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
  using JetBrains.Application;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.TextControl.Coords;
  using JetBrains.Util;

  /// <summary>The invert return value action handler.</summary>
  [UsedImplicitly]
  [ContextAction(Description = "Duplicates a method.", Name = "Duplicate method [Agent Johnson]", Priority = -1, Group = "C#")]
  public class DuplicateMethod : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateMethod"/> class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public DuplicateMethod([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>Determines whether this instance is available.</summary>
    /// <param name="element">The element.</param>
    /// <returns><c>true</c> if this instance is available; otherwise, <c>false</c>.</returns>
    public override bool IsAvailable(IElement element)
    {
      var typeMemberDeclaration = element.ToTreeNode().Parent as ITypeMemberDeclaration;
      if (typeMemberDeclaration == null)
      {
        return false;
      }

      var function = typeMemberDeclaration.DeclaredElement as IFunction;

      return function != null;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes action. Called after Update, that set <c>ActionPresentation.Enabled</c> to <c>true</c>.
    /// </summary>
    /// <param name="element">The element.</param>
    protected override void Execute(IElement element)
    {
      var textControl = this.TextControl;
      if (textControl == null)
      {
        return;
      }

      var selection = TextRange.InvalidRange;

      using (var cookie = this.EnsureWritable())
      {
        if (cookie.EnsureWritableResult != EnsureWritableResult.SUCCESS)
        {
          return;
        }

        ExecuteAction(element);
      }

      if (selection != TextRange.InvalidRange)
      {
        textControl.Selection.SetRange(TextControlPosRange.FromDocRange(textControl, selection.StartOffset, selection.EndOffset));
      }
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <returns>The context action text.</returns>
    protected override string GetText()
    {
      return "Duplicate method [Agent Johnson]";
    }

    /// <summary>Executes the specified element.</summary>
    /// <param name="element">The element.</param>
    /// <returns>Returns the text range.</returns>
    [NotNull]
    private static TextRange ExecuteAction([NotNull] IElement element)
    {
      var typeMemberDeclaration = element.ToTreeNode().Parent as ICSharpTypeMemberDeclaration;
      if (typeMemberDeclaration == null)
      {
        return TextRange.InvalidRange;
      }

      var classDeclaration = typeMemberDeclaration.GetContainingTypeDeclaration() as IClassDeclaration;
      if (classDeclaration == null)
      {
        return TextRange.InvalidRange;
      }

      var text = typeMemberDeclaration.GetText();

      var factory = CSharpElementFactory.GetInstance(element.GetPsiModule());
      if (factory == null)
      {
        return TextRange.InvalidRange;
      }

      var declaration = factory.CreateTypeMemberDeclaration(text) as IClassMemberDeclaration;
      if (declaration == null)
      {
        return TextRange.InvalidRange;
      }

      var anchor = typeMemberDeclaration as IClassMemberDeclaration;
      if (anchor == null)
      {
        return TextRange.InvalidRange;
      }

      var after = classDeclaration.AddClassMemberDeclarationAfter(declaration, anchor);
      if (after != null)
      {
        var treeTextRange = after.GetNameRange();
        return new TextRange(treeTextRange.StartOffset.Offset, treeTextRange.EndOffset.Offset);
      }

      return TextRange.InvalidRange;
    }

    #endregion
  }
}