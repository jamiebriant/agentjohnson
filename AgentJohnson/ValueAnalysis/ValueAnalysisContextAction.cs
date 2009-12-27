// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueAnalysisContextAction.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Context Action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using JetBrains.Util;

namespace AgentJohnson.ValueAnalysis
{
  using EnvDTE;
  using JetBrains.Annotations;
  using JetBrains.ReSharper.Intentions;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.VsIntegration.Application;

  /// <summary>
  /// Represents the Context Action.
  /// </summary>
  [ContextAction(Description = "Annotates a function with Value Analysis attributes and assert statements.", Name = "Annotate with Value Analysis attributes", Priority = 0, Group = "C#")]
  public class ValueAnalysisContextAction : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueAnalysisContextAction"/> class.
    /// </summary>
    /// <param name="provider">
    /// The provider.
    /// </param>
    public ValueAnalysisContextAction(ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes the internal.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    protected override void Execute(IElement element)
    {
      var typeMemberDeclaration = this.GetTypeMemberDeclaration();
      if (typeMemberDeclaration == null)
      {
        return;
      }

      var valueAnalysisRefactoring = new ValueAnalysisRefactoring(typeMemberDeclaration);

      valueAnalysisRefactoring.Execute();

      if (!ValueAnalysisSettings.Instance.ExecuteGhostDoc)
      {
        return;
      }
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <returns>
    /// The text of the action.
    /// </returns>
    /// <value>
    /// The text.
    /// </value>
    protected override string GetText()
    {
      return "Annotate with Value Analysis attributes [Agent Johnson]";
    }

    /// <summary>
    /// Determines whether this instance is available.
    /// </summary>
    /// <param name="cache">
    /// The element.
    /// </param>
    /// <returns>
    /// <c>true</c> if this instance is available; otherwise, <c>false</c>.
    /// </returns>
    public override bool IsAvailable(IUserDataHolder cache)
    {
      var typeMemberDeclaration = this.GetTypeMemberDeclaration();
      if (typeMemberDeclaration == null)
      {
        return false;
      }

      var valueAnalysisRefactoring = new ValueAnalysisRefactoring(typeMemberDeclaration);

      return valueAnalysisRefactoring.IsAvailable();
    }

    /// <summary>
    /// Posts the execute.
    /// </summary>
    protected override void PostExecute()
    {
      _DTE dte = VSShell.Instance.ServiceProvider.Dte();
      Command command;

      try
      {
        command = dte.Commands.Item("Tools.SubMain.GhostDoc.DocumentThis");
      }
      catch
      {
        command = null;
      }

      if (command != null)
      {
        dte.ExecuteCommand("Tools.SubMain.GhostDoc.DocumentThis", string.Empty);
      }
    }

    /// <summary>
    /// Gets the type member declaration.
    /// </summary>
    /// <returns>
    /// The type member declaration.
    /// </returns>
    [CanBeNull]
    private ITypeMemberDeclaration GetTypeMemberDeclaration()
    {
      var element = this.Provider.SelectedElement;
      if (element == null)
      {
        return null;
      }

      ITypeMemberDeclaration typeMemberDeclaration = null;

      var treeNode = element as ITreeNode;
      if (treeNode != null)
      {
        typeMemberDeclaration = treeNode.Parent as ITypeMemberDeclaration;
      }

      if (typeMemberDeclaration == null)
      {
        var identifierNode = element as IIdentifierNode;

        if (identifierNode != null)
        {
          typeMemberDeclaration = identifierNode.Parent as ITypeMemberDeclaration;
        }
      }

      return typeMemberDeclaration;
    }

    #endregion
  }
}