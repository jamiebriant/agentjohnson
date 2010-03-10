// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReturnAnalyzer.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The return analyzer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.ValueAnalysis
{
  using System.Collections.Generic;
  using JetBrains.ProjectModel;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.ControlFlow2;
  using JetBrains.ReSharper.Psi.ControlFlow2.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Util;

  /// <summary>The return analyzer.</summary>
  public class ReturnAnalyzer
  {
    #region Constants and Fields

    /// <summary>
    /// The solution.
    /// </summary>
    private readonly ISolution solution;

    #endregion

    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="ReturnAnalyzer"/> class.</summary>
    /// <param name="solution">The solution.</param>
    public ReturnAnalyzer(ISolution solution)
    {
      this.solution = solution;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the solution.
    /// </summary>
    /// <value>The solution.</value>
    private ISolution Solution
    {
      get
      {
        return this.solution;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Analyzes the return statement.
    /// </summary>
    /// <param name="returnStatement">The return statement.</param>
    /// <returns>Returns the return statement.</returns>
    public IEnumerable<SuggestionBase> AnalyzeReturnStatement(IReturnStatement returnStatement)
    {
      var suggestions = new List<SuggestionBase>();

      if (returnStatement.Value == null)
      {
        return suggestions;
      }

      if (this.RequiresAssertion(returnStatement))
      {
        suggestions.Add(new ReturnWarning(returnStatement));
      }

      return suggestions;
    }

    #endregion

    #region Methods

    /// <summary>Gets the value analysis.</summary>
    /// <param name="returnStatement">The return statement.</param>
    /// <param name="function">The function.</param>
    /// <returns>Returns the boolean.</returns>
    private static bool GetValueAnalysis(IReturnStatement returnStatement, IFunction function)
    {
      var referenceExpression = returnStatement.Value as IReferenceExpression;
      if (referenceExpression == null)
      {
        return false;
      }

      var functionDeclaration = function as ICSharpFunctionDeclaration;

      var graf = CSharpControlFlowBuilder.Build(functionDeclaration);

      var inspect = graf.Inspect(ValueAnalysisMode.OPTIMISTIC);

      var state = inspect.GetExpressionNullReferenceState(referenceExpression);

      switch (state)
      {
        case CSharpControlFlowNullReferenceState.UNKNOWN:
          return true;
        case CSharpControlFlowNullReferenceState.NOT_NULL:
          return false;
        case CSharpControlFlowNullReferenceState.NULL:
          return true;
        case CSharpControlFlowNullReferenceState.MAY_BE_NULL:
          return true;
      }

      return true;
    }

    /// <summary>Gets the is asserted.</summary>
    /// <param name="returnStatement">The return statement.</param>
    /// <returns>Returns the boolean.</returns>
    private bool GetIsAsserted(IReturnStatement returnStatement)
    {
      var invocationExpression = returnStatement.Value as IInvocationExpression;
      if (invocationExpression == null)
      {
        return false;
      }

      var invokedExpression = invocationExpression.InvokedExpression as IReferenceExpression;
      if (invokedExpression == null)
      {
        return false;
      }

      var resolveResult = invokedExpression.Reference.Resolve();

      IMethod method = null;

      var methodDeclaration = resolveResult.DeclaredElement as IMethodDeclaration;
      if (methodDeclaration != null)
      {
        method = methodDeclaration.DeclaredElement;
      }

      if (method == null)
      {
        method = resolveResult.DeclaredElement as IMethod;
      }

      if (method == null)
      {
        return false;
      }

      var codeAnnotationsCache = CodeAnnotationsCache.GetInstance(this.solution);

      return codeAnnotationsCache.IsAssertionMethod(method);
    }

    /// <summary>Determines whether this instance has annotation.</summary>
    /// <param name="function">The function.</param>
    /// <returns><c>true</c> if this instance has annotation; otherwise, <c>false</c>.</returns>
    private bool HasAnnotation(IFunction function)
    {
      var codeAnnotationsCache = CodeAnnotationsCache.GetInstance(this.Solution);

      var instances = function.GetAttributeInstances(true);
      foreach (var list in instances)
      {
        if (codeAnnotationsCache.IsAnnotationAttribute(list))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>Determines whether this instance is asserted.</summary>
    /// <param name="returnStatement">The return statement.</param>
    /// <returns><c>true</c> if this instance is asserted; otherwise, <c>false</c>.</returns>
    private bool RequiresAssertion(IReturnStatement returnStatement)
    {
      var canBeNullName = CodeAnnotationsCache.CanBeNullAttributeShortName;
      if (string.IsNullOrEmpty(canBeNullName))
      {
        return false;
      }

      var notNullName = CodeAnnotationsCache.NotNullAttributeShortName;
      if (string.IsNullOrEmpty(notNullName))
      {
        return false;
      }

      var value = returnStatement.Value;
      if (value.IsConstantValue())
      {
        return false;
      }

      var returnValue = value.GetText();
      if (returnValue == "string.Empty" || returnValue == "String.Empty" || returnValue == "null")
      {
        return false;
      }

      if (!(value is ICreationExpression))
      {
        return false;
      }

      var function = returnStatement.GetContainingTypeMemberDeclaration() as IFunction;
      if (function == null)
      {
        return false;
      }

      var type = function.ReturnType;
      if (!type.IsReferenceType())
      {
        return false;
      }

      if (this.HasAnnotation(function))
      {
        return false;
      }

      var rule = Rule.GetRule(type, function.Language) ?? Rule.GetDefaultRule();
      if (rule == null)
      {
        return false;
      }

      if (string.IsNullOrEmpty(rule.ReturnAssertion))
      {
        return false;
      }

      var isAsserted = this.GetIsAsserted(returnStatement);
      if (isAsserted)
      {
        return false;
      }

      return GetValueAnalysis(returnStatement, function);
    }

    #endregion
  }
}