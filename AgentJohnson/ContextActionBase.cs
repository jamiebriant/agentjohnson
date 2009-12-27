// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextActionBase.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the base of a context action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using JetBrains.Annotations;
using JetBrains.DocumentModel;
namespace AgentJohnson
{
  using System;
  using JetBrains.Application;
  using JetBrains.ProjectModel;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.TextControl;
  using JetBrains.Util;

  /// <summary>
  /// Represents the base of a context action.
  /// </summary>
  public abstract class ContextActionBase: IBulbItem, IContextAction
  {
    #region Constants and Fields

    /// <summary>
    /// The current provider.
    /// </summary>
    private readonly ICSharpContextActionDataProvider provider;
      public ICSharpContextActionDataProvider Provider
      {
          get { return provider; }
      }

    /// <summary>
    /// Indicates if transactions should be started.
    /// </summary>
    private bool startTransaction = true;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextActionBase"/> class.
    /// </summary>
    /// <param name="provider">
    /// The provider.
    /// </param>
    protected ContextActionBase(ICSharpContextActionDataProvider provider)
    {
        this.provider = provider;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    public IBulbItem[] Items
    {
      get
      {
        return new[]
        {
          this
        };
      }
    }

    /// <summary>
    /// Gets the solution.
    /// </summary>
    /// <value>The solution.</value>
    protected ISolution Solution
    {
      get
      {
        return this.provider.Solution;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [start transaction].
    /// </summary>
    /// <value><c>true</c> if [start transaction]; otherwise, <c>false</c>.</value>
    protected bool StartTransaction
    {
      get
      {
        return this.startTransaction;
      }

      set
      {
        this.startTransaction = value;
      }
    }

    /// <summary>
    /// Gets the text control.
    /// </summary>
    /// <value>The text control.</value>
    protected ITextControl TextControl
    {
      get
      {
        return this.provider.TextControl;
      }
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <value>The bulb text.</value>
    string IBulbItem.Text
    {
      get
      {
        return this.GetText();
      }
    }

    #endregion

    #region Implemented Interfaces

    #region IBulbItem

    /// <summary>
    /// Executes the specified solution.
    /// </summary>
    /// <param name="solution">The solution.</param>
    /// <param name="textControl">The text control.</param>
    void IBulbItem.Execute(ISolution solution, ITextControl textControl)
    {
      if (this.Solution != solution || this.TextControl != textControl)
      {
        throw new InvalidOperationException();
      }

      Shell.Instance.Locks.AssertReadAccessAllowed();

      var element = this.provider.SelectedElement;
      if (element == null)
      {
        return;
      }

      if (this.StartTransaction)
      {
        this.Modify(() => this.Execute(element));
      }
      else
      {
        this.Execute(element);
      }

      this.PostExecute();
    }

    #endregion

    #endregion

    #region Methods
    /// <summary>
    /// Returns the Modification Cookie.
    /// </summary>
    /// <returns>The cookie.</returns>
    [NotNull]
    protected ModificationCookie EnsureWritable()
    {
        if (Solution != null)
        {
            return DocumentManager.GetInstance(Solution).EnsureWritable(TextControl.Document);
        }
        return new ModificationCookie(EnsureWritableResult.FAILURE);
    }
    /// <summary>
    /// Executes this instance.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    protected abstract void Execute(IElement element);

    /// <summary>
    /// Called to apply context action. No locks is taken before call
    /// </summary>
    /// <param name="param">
    /// The parameters.
    /// </param>
    protected void ExecuteInternal(params object[] param)
    {
      var element = param[0] as IElement;

      if (this.startTransaction)
      {
        this.Modify(() => this.Execute(element));
      }
      else
      {
        this.Execute(element);
      }

      this.PostExecute();
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <returns>
    /// The context action text.
    /// </returns>
    protected abstract string GetText();

    /// <summary>
    /// Determines whether this instance is available.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    /// <returns>
    /// <c>true</c> if this instance is available; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool IsAvailable(IUserDataHolder element);

    /// <summary>
    /// Called to check if ContextAction is available.
    /// ReadLock is taken
    /// Will not be called if <c>PsiManager</c>, ProjectFile of Solution == <c>null</c>
    /// </summary>
    /// <returns>
    /// The is available internal.
    /// </returns>
    protected bool IsAvailableInternal()
    {
      Shell.Instance.Locks.AssertReadAccessAllowed();

      var element = this.provider.SelectedElement;
      if (element == null)
      {
        return false;
      }

      return this.IsAvailable(element);
    }

    /// <summary>
    /// Posts the execute.
    /// </summary>
    protected virtual void PostExecute()
    {
    }

    /// <summary>
    /// Modifies the specified handler.
    /// </summary>
    /// <param name="handler">
    /// The handler.
    /// </param>
    private void Modify(Action handler)
    {
      var psiManager = PsiManager.GetInstance(this.Solution);
      if (psiManager == null)
      {
        return;
      }

      using (var cookie = EnsureWritable())
      {
        if (cookie.EnsureWritableResult != EnsureWritableResult.SUCCESS)
        {
          return;
        }

        using (CommandCookie.Create(string.Format("Context Action {0}", this.GetText())))
        {
          psiManager.DoTransaction(handler);
        }
      }
    }

    #endregion
  }
}