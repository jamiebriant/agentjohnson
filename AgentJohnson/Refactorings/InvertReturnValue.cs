// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvertReturnValueActionHandler.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The invert return value action handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.Refactorings
{
  using JetBrains.Annotations;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>The invert return value action handler.</summary>
  [UsedImplicitly]
  [ContextAction(Description = "Inverts the return boolean value.", Name = "Invert return value [Agent Johnson]", Priority = -1, Group = "C#")]
  public class InvertReturnValue : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="InvertReturnValue"/> class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public InvertReturnValue([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Executes action. Called after Update, that set ActionPresentation.Enabled to true.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>
    /// 	<c>true</c> if this instance is available; otherwise, <c>false</c>.
    /// </returns>
    public override bool IsAvailable(IElement element)
    {
      return InvertReturnValueRefactoring.IsAvailable(element);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the specified context.
    /// </summary>
    /// <param name="element">The element.</param>
    protected override void Execute(IElement element)
    {
      var invertReturnValueRefactoring = new InvertReturnValueRefactoring(this.Solution, this.TextControl);

      invertReturnValueRefactoring.Execute();
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <returns>The context action text.</returns>
    protected override string GetText()
    {
      return "Invert return value [Agent Johnson]";
    }

    #endregion
  }
}