// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NegateIfCondition.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the negate if condition class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using JetBrains.Util;

namespace AgentJohnson.Statements
{
  using JetBrains.ReSharper.Intentions;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.ReSharper.Psi.CSharp.Util;

  /// <summary>
  /// Defines the negate if condition class.
  /// </summary>
  [ContextAction(Description = "Negates the condition of an 'if' statement.", Name = "Negate 'if' condition", Priority = -1, Group = "C#")]
  public class NegateIfCondition : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="NegateIfCondition"/> class.
    /// </summary>
    /// <param name="provider">
    /// The provider.
    /// </param>
    public NegateIfCondition(ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes this instance.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    protected override void Execute(IElement element)
    {
      Negate(element);
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <returns>
    /// The text in the context menu.
    /// </returns>
    protected override string GetText()
    {
      return "Negate 'if' condition [Agent Johnson]";
    }

    /// <summary>
    /// Determines whether this instance is available.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    /// <returns>
    /// <c>true</c> if this instance is available; otherwise, <c>false</c>.
    /// </returns>
    public override bool IsAvailable(IUserDataHolder element)
    {
        var ifStatement = this.Provider.GetSelectedElement<IIfStatement>(false, true);
      if (ifStatement == null)
      {
        return false;
      }

      IExpression condition = ifStatement.Condition;
      if (condition == null)
      {
        return false;
      }

      if (!condition.Contains(ifStatement))
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Reverses the specified element.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    private static void Negate(IElement element)
    {
      var ifStatement = element.GetContainingElement<IIfStatement>(true);
      if (ifStatement == null)
      {
        return;
      }

      IExpression condition = ifStatement.Condition;
      if (condition == null)
      {
        return;
      }

      var factory = CSharpElementFactory.GetInstance(element.GetPsiModule());
      if (factory == null)
      {
        return;
      }

      var equalityExpression = ifStatement.Condition as IEqualityExpression;
      if (equalityExpression != null)
      {
        NegateEqualityExpression(factory, equalityExpression);
        return;
      }

      var relationalExpression = ifStatement.Condition as IRelationalExpression;
      if (relationalExpression != null)
      {
        NegateRelationalExpression(factory, relationalExpression);
        return;
      }

      var unaryOperatorExpression = ifStatement.Condition as IUnaryOperatorExpression;
      if (unaryOperatorExpression != null)
      {
        NegateUnaryExpression(factory, unaryOperatorExpression);
        return;
      }

      var invocationExpression = ifStatement.Condition as IInvocationExpression;
      if (invocationExpression != null)
      {
        NegateInvocationExpression(factory, invocationExpression);
        return;
      }

      var literalExpression = ifStatement.Condition as ILiteralExpression;
      if (literalExpression != null)
      {
        NegateLiteralExpression(factory, literalExpression);
        return;
      }

      NegateExpression(factory, ifStatement.Condition);
    }

    /// <summary>
    /// Negates the equality expression.
    /// </summary>
    /// <param name="factory">
    /// The factory.
    /// </param>
    /// <param name="equalityExpression">
    /// The equality expression.
    /// </param>
    private static void NegateEqualityExpression(CSharpElementFactory factory, IEqualityExpression equalityExpression)
    {
      var operatorSign = equalityExpression.OperatorSign.GetText();

      operatorSign = operatorSign == "==" ? "!=" : "==";

      var expression = factory.CreateExpression(string.Format("{0} {1} {2}", equalityExpression.LeftOperand.GetText(), operatorSign, equalityExpression.RightOperand.GetText()));

      equalityExpression.ReplaceBy(expression);
    }

    /// <summary>
    /// Negates the expression.
    /// </summary>
    /// <param name="factory">
    /// The factory.
    /// </param>
    /// <param name="condition">
    /// The condition.
    /// </param>
    private static void NegateExpression(CSharpElementFactory factory, ICSharpExpression condition)
    {
      var expression = factory.CreateExpression("!(" + condition.GetText() + ")");

      condition.ReplaceBy(expression);
    }

    /// <summary>
    /// Negates the invocation expression.
    /// </summary>
    /// <param name="factory">
    /// The factory.
    /// </param>
    /// <param name="invocationExpression">
    /// The invocation expression.
    /// </param>
    private static void NegateInvocationExpression(CSharpElementFactory factory, IInvocationExpression invocationExpression)
    {
      var expression = factory.CreateExpression("!" + invocationExpression.GetText());

      invocationExpression.ReplaceBy(expression);
    }

    /// <summary>
    /// Negates the literal expression.
    /// </summary>
    /// <param name="factory">
    /// The factory.
    /// </param>
    /// <param name="literalExpression">
    /// The literal expression.
    /// </param>
    private static void NegateLiteralExpression(CSharpElementFactory factory, ILiteralExpression literalExpression)
    {
      ICSharpExpression csharpExpression = literalExpression as ICSharpExpression;
      if (csharpExpression == null)
      {
        return;
      }

      var text = literalExpression.GetText();

      if (text == "true")
      {
        text = "false";
      }
      else if (text == "false")
      {
        text = "true";
      }
      else
      {
        return;
      }

      var expression = factory.CreateExpression(text);

      ExpressionUtil.ReplaceExpression(csharpExpression, expression);
    }

    /// <summary>
    /// Negates the relational expression.
    /// </summary>
    /// <param name="factory">
    /// The factory.
    /// </param>
    /// <param name="relationalExpression">
    /// The relational expression.
    /// </param>
    private static void NegateRelationalExpression(CSharpElementFactory factory, IRelationalExpression relationalExpression)
    {
      var operatorSign = relationalExpression.OperatorSign.GetText();

      switch (operatorSign)
      {
        case "<":
          operatorSign = ">=";
          break;
        case ">":
          operatorSign = "<=";
          break;
        case "<=":
          operatorSign = ">";
          break;
        case ">=":
          operatorSign = "<";
          break;
      }

      var expression = factory.CreateExpression(string.Format("{0} {1} {2}", relationalExpression.LeftOperand.GetText(), operatorSign, relationalExpression.RightOperand.GetText()));

      relationalExpression.ReplaceBy(expression);
    }

    /// <summary>
    /// Negates the unary expression.
    /// </summary>
    /// <param name="factory">
    /// The factory.
    /// </param>
    /// <param name="unaryOperatorExpression">
    /// The unary operator expression.
    /// </param>
    private static void NegateUnaryExpression(CSharpElementFactory factory, IUnaryOperatorExpression unaryOperatorExpression)
    {
      if (unaryOperatorExpression.OperatorSign.GetText() != "!")
      {
        return;
      }

      var text = unaryOperatorExpression.Operand.GetText().Trim();

      if (text.StartsWith("(") && text.EndsWith(")"))
      {
        text = text.Substring(1, text.Length - 2);
      }

      var expression = factory.CreateExpression(text);

      unaryOperatorExpression.ReplaceBy(expression);
    }

    #endregion
  }
}