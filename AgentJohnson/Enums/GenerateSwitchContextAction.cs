// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenerateSwitchContextAction.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Context Action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.Enums
{
  using System.Text;
  using AgentJohnson.Psi.CodeStyle;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.ReSharper.Psi.Util;

  /// <summary>Represents the Context Action.</summary>
  [ContextAction(Description = "Generates a 'switch' statement based on the current 'enum' expression.", Name = "Generate 'switch' statement [Agent Johnson]", Priority = -1, Group = "C#")]
  public class GenerateSwitchContextAction : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="GenerateSwitchContextAction"/> class.</summary>
    /// <param name="provider">The provider.</param>
    public GenerateSwitchContextAction(ICSharpContextActionDataProvider provider) : base(provider)
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
      var statement = this.Provider.GetSelectedElement<IExpressionStatement>(true, true);
      if (statement == null)
      {
        return false;
      }

      var expression = statement.Expression;
      if ((expression == null) || !(expression.ToTreeNode() is IUnaryExpressionNode))
      {
        return false;
      }

      var type = expression.Type();
      if (!type.IsResolved)
      {
        return false;
      }

      return type.IsEnumType();
    }

    #endregion

    #region Methods

    /// <summary>Executes the internal.</summary>
    /// <param name="element">The element.</param>
    protected override void Execute(IElement element)
    {
      var statement = this.Provider.GetSelectedElement<IExpressionStatement>(true, true);
      if (statement == null)
      {
        return;
      }

      var factory = CSharpElementFactory.GetInstance(statement.GetPsiModule());
      if (factory == null)
      {
        return;
      }

      var expression = statement.Expression;
      if ((expression == null) || !(expression.ToTreeNode() is IUnaryExpressionNode))
      {
        return;
      }

      var type = expression.Type();
      if (!type.IsResolved)
      {
        return;
      }

      var declaredType = type as IDeclaredType;
      if (declaredType == null)
      {
        return;
      }

      var enumerate = declaredType.GetTypeElement() as IEnum;
      if (enumerate == null)
      {
        return;
      }

      var typeName = enumerate.ShortName;

      var code = new StringBuilder("switch(");

      code.Append(statement.GetText());

      code.Append(") {");

      foreach (var field in enumerate.EnumMembers)
      {
        code.Append("case ");
        code.Append(typeName);
        code.Append('.');
        code.Append(field.ShortName);
        code.Append(":\r\nbreak;");
      }

      code.Append("default:\r\n");
      code.Append("throw new System.ArgumentOutOfRangeException();");

      code.Append("\r\n}");

      var result = factory.CreateStatement(code.ToString());
      if (result == null)
      {
        return;
      }

      result = statement.ReplaceBy(result);

      var range = result.GetDocumentRange();

      var codeFormatter = new CodeFormatter();
      codeFormatter.Format(this.Solution, range);
    }

    /// <summary>Gets the text.</summary>
    /// <value>The text.</value>
    /// <returns>The get text.</returns>
    protected override string GetText()
    {
      return string.Format("Generate 'switch' statement [Agent Johnson]");
    }

    #endregion
  }
}