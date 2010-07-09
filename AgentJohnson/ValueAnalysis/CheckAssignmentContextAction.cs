// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CheckAssignmentContextAction.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Context Action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.ValueAnalysis
{
  using AgentJohnson.Psi.CodeStyle;
  using JetBrains.Annotations;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.ReSharper.Psi.Util;
  using JetBrains.Util;

  /// <summary>Represents the Context Action.</summary>
  [ContextAction(Description = "Adds an 'if' statement after the current statement that checks if the variable is null.", Name = "Check if variable is null [Agent Johnson]", Priority = -1, Group = "C#")]
  public class CheckAssignmentContextAction : ContextActionBase
  {
    #region Constants and Fields

    /// <summary>
    /// The _name.
    /// </summary>
    private string name;

    #endregion

    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="CheckAssignmentContextAction"/> class.</summary>
    /// <param name="provider">The provider.</param>
    public CheckAssignmentContextAction(ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>Called to check if ContextAction is available.
    /// ReadLock is taken
    /// Will not be called if <c>PsiManager</c>, ProjectFile of Solution == null</summary>
    /// <param name="element">The element.</param>
    /// <returns>The is available.</returns>
    public override bool IsAvailable(IElement element)
    {
      this.name = null;

      var localVariableDeclaration = this.Provider.GetSelectedElement<ILocalVariableDeclaration>(true, true);
      var assignmentExpression = this.Provider.GetSelectedElement<IAssignmentExpression>(true, true);

      if (assignmentExpression == null && localVariableDeclaration == null)
      {
        return false;
      }

      TextRange range;
      IType declaredType;

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

        var referenceExpression = destination as IReferenceExpression;
        if (referenceExpression == null)
        {
          return false;
        }

        var reference = referenceExpression.Reference;
        var source = assignmentExpression.Source;
        if (source == null)
        {
          return false;
        }

        this.name = reference.GetName();

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

        var declNode = localVariableDeclaration.ToTreeNode();
        if (declNode.AssignmentSign == null)
        {
          return false;
        }

        this.name = localVariable.ShortName;

        IIdentifierNode identifier = declNode.NameIdentifier;
        if (identifier == null)
        {
          return false;
        }

        var initial = localVariableDeclaration.Initial;
        if (initial == null)
        {
          return false;
        }

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

      return range.IsValid && range.Contains(this.Provider.CaretOffset.Offset);
    }

    #endregion

    #region Methods

    /// <summary>Executes the internal.</summary>
    /// <param name="element">The element.</param>
    protected override void Execute([NotNull] IElement element)
    {
      if (!this.IsAvailable(element))
      {
        return;
      }

      var assignmentExpression = this.Provider.GetSelectedElement<IAssignmentExpression>(true, true);
      if (assignmentExpression != null)
      {
        this.CheckAssignment(assignmentExpression);
        return;
      }

      var localVariableDeclaration = this.Provider.GetSelectedElement<ILocalVariableDeclaration>(true, true);
      if (localVariableDeclaration != null)
      {
        this.CheckAssignment(localVariableDeclaration);
      }
    }

    /// <summary>Gets the text.</summary>
    /// <returns>The get text.</returns>
    /// <value>The text.</value>
    [NotNull]
    protected override string GetText()
    {
      return string.Format("Check if '{0}' is null [Agent Johnson]", this.name ?? "[unknown]");
    }

    /// <summary>Inserts the assertion code.</summary>
    /// <param name="localVariableDeclaration">The local variable declaration.</param>
    private void CheckAssignment([NotNull] ILocalVariableDeclaration localVariableDeclaration)
    {
      var localVariable = localVariableDeclaration.DeclaredElement as ILocalVariable;
      if (localVariable == null)
      {
        return;
      }

      IStatement anchor = null;

      ITreeNode treeNode = localVariableDeclaration.ToTreeNode();

      while (treeNode != null)
      {
        anchor = treeNode as IStatement;

        if (anchor != null)
        {
          break;
        }

        treeNode = treeNode.Parent;
      }

      if (anchor == null)
      {
        return;
      }

      this.CheckAssignment(localVariableDeclaration, anchor, localVariable.ShortName);
    }

    /// <summary>Inserts the assertion code.</summary>
    /// <param name="assignmentExpression">The assignment expression.</param>
    private void CheckAssignment([NotNull] IAssignmentExpression assignmentExpression)
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

      var anchor = assignmentExpression.GetContainingStatement();

      if (anchor != null)
      {
        this.CheckAssignment(assignmentExpression, anchor, referenceExpression.Reference.GetName());
      }
    }

    /// <summary>Inserts the assert.</summary>
    /// <param name="element">The element.</param>
    /// <param name="anchor">The anchor.</param>
    /// <param name="assignmentName">The assignmentName.</param>
    private void CheckAssignment([NotNull] IElement element, [NotNull] IStatement anchor, [NotNull] string assignmentName)
    {
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

      var code = string.Format("if({0} == null) {{ }}", assignmentName);

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

    #endregion
  }
}