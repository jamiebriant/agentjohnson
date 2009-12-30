// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuplicateMethod.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The invert return value action handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AgentJohnson.Statements
{
  using JetBrains.Annotations;
  using JetBrains.ActionManagement;
  using JetBrains.Application;
  using JetBrains.IDE;
  using JetBrains.ProjectModel;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.TextControl.Coords;

  /// <summary>
  /// The invert return value action handler.
  /// </summary>
    [ActionHandler("AgentJohnson.DuplicateMethod")]
    public class DuplicateMethodActionHandler : ActionHandlerBase
  {
    #region Methods

    /// <summary>
    /// Executes action. Called after Update, that set <c>ActionPresentation.Enabled</c> to <c>true</c>.
    /// </summary>
    /// <param name="solution">
    /// The solution.
    /// </param>
    /// <param name="context">
    /// The context.
    /// </param>
    protected override void Execute([NotNull] ISolution solution, [NotNull] IDataContext context)
    {
      if (!context.CheckAllNotNull(DataConstants.SOLUTION))
      {
        return;
      }

      var element = GetElementAtCaret(context);
      if (element == null)
      {
        return;
      }

      Shell.Instance.Locks.AssertReadAccessAllowed();

      var textControl = context.GetData(DataConstants.TEXT_CONTROL);
      if (textControl == null)
      {
        return;
      }

      var selection = JetBrains.Util.TextRange.InvalidRange;


      using (var cookie = EnsureWritable(solution, textControl.Document))
      {
        if (cookie.EnsureWritableResult != JetBrains.Util.EnsureWritableResult.SUCCESS)
        {
          return;
        }

        using (CommandCookie.Create("Context Action Duplicate Method"))
        {
          PsiManager.GetInstance(solution).DoTransaction(delegate { selection = Execute(element); });
        }
      }

      if (selection != JetBrains.Util.TextRange.InvalidRange)
      {
        textControl.Selection.SetRange(TextControlPosRange.FromDocRange(textControl, selection.StartOffset, selection.EndOffset));
      }
    }

    /// <summary>
    /// Updates the specified context.
    /// </summary>
    /// <param name="context">
    /// The context.
    /// </param>
    /// <returns>
    /// The update.
    /// </returns>
    protected override bool Update([NotNull] IDataContext context)
    {
      if (!context.CheckAllNotNull(DataConstants.SOLUTION))
      {
        return false;
      }

      var element = GetElementAtCaret(context);
      if (element == null)
      {
        return false;
      }

      Shell.Instance.Locks.AssertReadAccessAllowed();

      var typeMemberDeclaration = element.ToTreeNode().Parent as ITypeMemberDeclaration;
      if (typeMemberDeclaration == null)
      {
        return false;
      }

      var function = typeMemberDeclaration.DeclaredElement as IFunction;

      return function != null;
    }

    /// <summary>
    /// Executes the specified element.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    /// <returns>
    /// Returns the text range.
    /// </returns>
    [NotNull]
    private static JetBrains.Util.TextRange Execute([NotNull] IElement element)
    {
      var typeMemberDeclaration = element.ToTreeNode().Parent as ICSharpTypeMemberDeclaration;
      if (typeMemberDeclaration == null)
      {
        return JetBrains.Util.TextRange.InvalidRange;
      }

      var classDeclaration = typeMemberDeclaration.GetContainingTypeDeclaration() as IClassDeclaration;
      if (classDeclaration == null)
      {
        return JetBrains.Util.TextRange.InvalidRange;
      }

      var text = typeMemberDeclaration.GetText();

      var factory = CSharpElementFactory.GetInstance(element.GetPsiModule());
      if (factory == null)
      {
        return JetBrains.Util.TextRange.InvalidRange;
      }

      var declaration = factory.CreateTypeMemberDeclaration(text) as IClassMemberDeclaration;
      if (declaration == null)
      {
        return JetBrains.Util.TextRange.InvalidRange;
      }

      var anchor = typeMemberDeclaration as IClassMemberDeclaration;
      if (anchor == null)
      {
        return JetBrains.Util.TextRange.InvalidRange;
      }

      var after = classDeclaration.AddClassMemberDeclarationAfter(declaration, anchor);
      if (after != null)
      {
        var treeTextRange = after.GetNameRange();
        return new JetBrains.Util.TextRange(treeTextRange.StartOffset.Offset, treeTextRange.EndOffset.Offset);
      }

      return JetBrains.Util.TextRange.InvalidRange;
    }

    #endregion
  }
}