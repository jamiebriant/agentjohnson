// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssertAssignmentContextAction.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Context Action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.ValueAnalysis
{
  using System.Collections.Generic;
  using AgentJohnson.Psi.CodeStyle;
  using JetBrains.Application;
  using JetBrains.CommonControls;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.ReSharper.Psi.Util;
  using JetBrains.UI.PopupMenu;
  using JetBrains.Util;

  /// <summary>Represents the Context Action.</summary>
  [ContextAction(Description = "Adds an assertion statement after the current statement.", Name = "Assert assignment [Agent Johnson]", Priority = 0, Group = "C#")]
  public class AssertAssignmentContextAction : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="AssertAssignmentContextAction"/> class.</summary>
    /// <param name="provider">The provider.</param>
    public AssertAssignmentContextAction(ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    private string Name { get; set; }

    #endregion

    #region Public Methods

    /// <summary>Called to check if ContextAction is available.
    /// ReadLock is taken
    /// Will not be called if <c>PsiManager</c>, ProjectFile of Solution == null</summary>
    /// <param name="element">The element.</param>
    /// <returns><c>true</c> if this instance is available; otherwise, <c>false</c>.</returns>
    public override bool IsAvailable(IElement element)
    {
      var localVariableDeclaration = this.Provider.GetSelectedElement<ILocalVariableDeclaration>(true, true);
      var assignmentExpression = this.Provider.GetSelectedElement<IAssignmentExpression>(true, true);

      if (assignmentExpression == null && localVariableDeclaration == null)
      {
        return false;
      }

      TextRange range;
      IType declaredType;
      PsiLanguageType language;

      if (assignmentExpression != null)
      {
        var destination = assignmentExpression.Dest;
        if (destination == null)
        {
          return false;
        }

        if (!destination.IsClassifiedAsVariable)
        {
          return false;
        }

        declaredType = destination.GetExpressionType() as IDeclaredType;
        language = destination.Language;

        var referenceExpression = destination as IReferenceExpression;
        if (referenceExpression == null)
        {
          return false;
        }

        var reference = referenceExpression.Reference;
        IExpression source = assignmentExpression.Source;
        if (source == null)
        {
          return false;
        }

        this.Name = reference.GetName();

        range = new TextRange(destination.GetTreeStartOffset().Offset, source.GetTreeStartOffset().Offset);
      }
      else
      {
        var localVariable = localVariableDeclaration.DeclaredElement as ILocalVariable;
        if (localVariable == null)
        {
          return false;
        }

        declaredType = localVariable.Type;
        language = localVariable.Language;

        var declNode = localVariableDeclaration.ToTreeNode();
        if (declNode.AssignmentSign == null)
        {
          return false;
        }

        var initial = localVariableDeclaration.Initial;
        if (initial == null)
        {
          return false;
        }

        IIdentifierNode identifier = declNode.NameIdentifier;
        if (identifier == null)
        {
          return false;
        }

        this.Name = localVariable.ShortName;

        range = new TextRange(identifier.GetTreeStartOffset().Offset, initial.GetTreeStartOffset().Offset);
      }

      if (declaredType == null)
      {
        return false;
      }

      if (!declaredType.IsReferenceType())
      {
        return false;
      }

      if (!range.IsValid || !range.Contains(this.Provider.CaretOffset.Offset))
      {
        return false;
      }

      var rule = Rule.GetRule(declaredType, language) ?? Rule.GetDefaultRule();
      if (rule == null)
      {
        return false;
      }

      return rule.ValueAssertions.Count > 0;
    }

    #endregion

    #region Methods

    /// <summary>Executes the internal.</summary>
    /// <param name="element">The element.</param>
    protected override void Execute(IElement element)
    {
      if (!this.IsAvailable(element))
      {
        return;
      }

      var assignmentExpression = this.Provider.GetSelectedElement<IAssignmentExpression>(true, true);
      if (assignmentExpression != null)
      {
        this.InsertAssertionCode(assignmentExpression);
        return;
      }

      var localVariableDeclaration = this.Provider.GetSelectedElement<ILocalVariableDeclaration>(true, true);
      if (localVariableDeclaration != null)
      {
        this.InsertAssertionCode(localVariableDeclaration);
      }
    }

    /// <summary>Gets the text.</summary>
    /// <returns>The context action text.</returns>
    /// <value>The context action text.</value>
    protected override string GetText()
    {
      return string.Format("Assert assignment to '{0}' [Agent Johnson]", this.Name);
    }

    /// <summary>Menu_s the item clicked.</summary>
    /// <param name="assertion">The assertion.</param>
    /// <param name="element">The element.</param>
    private void InsertAssertion(string assertion, IElement element)
    {
      var psiManager = PsiManager.GetInstance(this.Solution);
      if (psiManager == null)
      {
        return;
      }

      using (ReadLockCookie.Create())
      {
        using (var cookie = this.EnsureWritable())
        {
          if (cookie.EnsureWritableResult != EnsureWritableResult.SUCCESS)
          {
            return;
          }

          using (CommandCookie.Create(string.Format("Context Action {0}", this.GetText())))
          {
            psiManager.DoTransaction(() => this.InsertAssertionCode(assertion, element));
          }
        }
      }
    }

    /// <summary>Inserts the assert.</summary>
    /// <param name="assertion">The assertion.</param>
    /// <param name="element">The element.</param>
    private void InsertAssertionCode(string assertion, IElement element)
    {
      IStatement anchor = null;
      string name;

      var assignmentExpression = element as IAssignmentExpression;
      if (assignmentExpression != null)
      {
        anchor = assignmentExpression.GetContainingStatement();

        var referenceExpression = assignmentExpression.Dest as IReferenceExpression;
        if (referenceExpression == null)
        {
          return;
        }

        name = referenceExpression.Reference.GetName();
      }
      else
      {
        var treeNode = element.ToTreeNode();

        while (treeNode != null)
        {
          anchor = treeNode as IStatement;

          if (anchor != null)
          {
            break;
          }

          treeNode = treeNode.Parent;
        }

        var localVariable = element as ILocalVariable;
        if (localVariable == null)
        {
          return;
        }

        name = localVariable.ShortName;
      }

      if (anchor == null)
      {
        return;
      }

      var functionDeclaration = anchor.GetContainingTypeMemberDeclaration() as IMethodDeclaration;
      if (functionDeclaration == null)
      {
        return;
      }

      var body = functionDeclaration.Body;
      if (body == null)
      {
        return;
      }

      var factory = CSharpElementFactory.GetInstance(element.GetPsiModule());

      var csharpElement = element as ICSharpElement;
      if (csharpElement == null)
      {
        return;
      }

      var code = string.Format(assertion, name);

      var statement = factory.CreateStatement(code);
      if (statement == null)
      {
        return;
      }

      var result = body.AddStatementAfter(statement, anchor);

      var range = result.GetDocumentRange();
      var codeFormatter = new CodeFormatter();
      codeFormatter.Format(this.Solution, range);
    }

    /// <summary>Inserts the assertion code.</summary>
    /// <param name="localVariableDeclaration">The local variable declaration.</param>
    private void InsertAssertionCode(ILocalVariableDeclaration localVariableDeclaration)
    {
      var localVariable = localVariableDeclaration.DeclaredElement as ILocalVariable;
      if (localVariable == null)
      {
        return;
      }

      this.InsertAssertionCode(localVariable.Type, localVariableDeclaration, localVariable.ShortName);
    }

    /// <summary>Inserts the assertion code.</summary>
    /// <param name="assignmentExpression">The assignment expression.</param>
    private void InsertAssertionCode(IAssignmentExpression assignmentExpression)
    {
      var destination = assignmentExpression.Dest;
      if (destination == null)
      {
        return;
      }

      if (!destination.IsClassifiedAsVariable)
      {
        return;
      }

      var type = destination.GetExpressionType() as IType;
      if (type == null)
      {
        return;
      }

      var referenceExpression = assignmentExpression.Dest as IReferenceExpression;
      if (referenceExpression == null)
      {
        return;
      }

      this.InsertAssertionCode(type, assignmentExpression, referenceExpression.Reference.GetName());
    }

    /// <summary>Inserts the assertion code.</summary>
    /// <param name="type">The type.</param>
    /// <param name="element">The element.</param>
    /// <param name="name">The name.</param>
    private void InsertAssertionCode(IType type, IElement element, string name)
    {
      var rule = Rule.GetRule(type, element.Language) ?? Rule.GetDefaultRule();
      if (rule == null)
      {
        return;
      }

      if (rule.ValueAssertions.Count == 1)
      {
        var valueAssertion = rule.ValueAssertions[0];

        this.InsertAssertionCode(valueAssertion, element);

        return;
      }

      this.ShowPopupMenu(element, rule, name);
    }

    /// <summary>Shows the popup menu.</summary>
    /// <param name="element">The element.</param>
    /// <param name="rule">The rule.</param>
    /// <param name="name">The name.</param>
    private void ShowPopupMenu(IElement element, Rule rule, string name)
    {
      var menu = new JetPopupMenu();

      var assertions = new List<SimpleMenuItem>(rule.ValueAssertions.Count);

      foreach (var valueAssertion in rule.ValueAssertions)
      {
        var item = new SimpleMenuItem
        {
          Text = string.Format(valueAssertion, name), 
          Style = MenuItemStyle.Enabled
        };

        item.Clicked += delegate { this.InsertAssertion(item.Text, element); };

        assertions.Add(item);
      }

      menu.Caption.Value = WindowlessControl.Create("Assert Assignment");
      menu.SetItems(assertions.ToArray());
      menu.KeyboardAcceleration.SetValue(KeyboardAccelerationFlags.Mnemonics);

      menu.Show();
    }

    #endregion
  }
}