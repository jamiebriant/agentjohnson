// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BoolExpression.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the generate boolean expression class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.Generators
{
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>Defines the generate boolean expression class.</summary>
  [SmartGenerate("Surround with 'if'", "Surrounds the boolean expression with 'if'.", Priority = -20)]
  public class BooleanExpression : SmartGenerateHandlerBase
  {
    #region Methods

    /// <summary>Gets the items.</summary>
    /// <param name="smartGenerateParameters">The get menu items parameters.</param>
    protected override void GetItems(SmartGenerateParameters smartGenerateParameters)
    {
      var element = smartGenerateParameters.Element;

      var expression = element.GetContainingElement(typeof(IExpression), false) as IExpression;
      while (expression != null)
      {
        var type = expression.Type();

        var typeName = type.GetPresentableName(element.Language);

        if (typeName == "bool")
        {
          this.AddAction("Surround with 'if'", "FA4B31AF-393D-44DB-93D3-F7E48BF97C53", expression.GetDocumentRange().TextRange);
          return;
        }

        expression = expression.GetContainingElement(typeof(IExpression), false) as IExpression;
      }
    }

    #endregion
  }
}