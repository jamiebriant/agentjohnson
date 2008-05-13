using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow2.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Util;

namespace AgentJohnson.ValueAnalysis {
  /// <summary>
  /// 
  /// </summary>
  public class ReturnAnalyzer : IStatementAnalyzer {
    #region Fields

    readonly ISolution _solution;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ReturnAnalyzer"/> class.
    /// </summary>
    /// <param name="solution">The solution.</param>
    public ReturnAnalyzer(ISolution solution) {
      _solution = solution;
    }

    #endregion

    #region Public properties

    /// <summary>
    /// Gets the solution.
    /// </summary>
    /// <value>The solution.</value>
    public ISolution Solution {
      get {
        return _solution;
      }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Analyzes the specified statement.
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <returns></returns>
    public SuggestionBase[] Analyze(IStatement statement) {
      List<SuggestionBase> suggestions = new List<SuggestionBase>();

      IReturnStatement returnStatement = statement as IReturnStatement;
      if(returnStatement != null) {
        suggestions.AddRange(AnalyzeReturnStatement(returnStatement));
      }

      return suggestions.ToArray();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Analyzes the return statement.
    /// </summary>
    /// <param name="returnStatement">The return statement.</param>
    /// <returns></returns>
    IEnumerable<SuggestionBase> AnalyzeReturnStatement(IReturnStatement returnStatement) {
      List<SuggestionBase> suggestions = new List<SuggestionBase>();

      if(returnStatement.Value == null) {
        return suggestions;
      }

      if(!IsAsserted(returnStatement)) {
        suggestions.Add(new ReturnWarning(_solution, returnStatement));
      }

      return suggestions;
    }

    /// <summary>
    /// Determines whether this instance is asserted.
    /// </summary>
    /// <param name="returnStatement">The return statement.</param>
    /// <returns>
    /// 	<c>true</c> if this instance is asserted; otherwise, <c>false</c>.
    /// </returns>
    bool IsAsserted(IReturnStatement returnStatement) {
      string canBeNullName = CodeAnnotationsCache.CanBeNullAttributeShortName;
      if(string.IsNullOrEmpty(canBeNullName)) {
        return true;
      }
      string notNullName = CodeAnnotationsCache.NotNullAttributeShortName;
      if(string.IsNullOrEmpty(notNullName)) {
        return true;
      }

      if(returnStatement.Value.IsConstantValue()) {
        return true;
      }

      string returnValue = returnStatement.Value.GetText();
      if (returnValue == "string.Empty" || returnValue == "String.Empty") {
        return true;
      }

      ICreationExpression creationExpression = returnStatement.Value as ICreationExpression;
      if(creationExpression != null) {
        return true;
      }

      IFunction function = returnStatement.GetContainingTypeMemberDeclaration() as IFunction;
      if(function == null) {
        return true;
      }

      IType type = function.ReturnType;
      if(!type.IsReferenceType()) {
        return true;
      }

      string canBeNullValueAttributeTypeName = CodeAnnotationsCache.GetInstance(Solution).GetDefaultAnnotationAttribute(canBeNullName);
      CLRTypeName canBeNullTypeName = new CLRTypeName(canBeNullValueAttributeTypeName);
      IList<IAttributeInstance> instances = function.GetAttributeInstances(canBeNullTypeName, true);
      if(instances != null && instances.Count > 0) {
        return true;
      }

      string notNullValueAttributeTypeName = CodeAnnotationsCache.GetInstance(Solution).GetDefaultAnnotationAttribute(notNullName);
      CLRTypeName notNullTypeName = new CLRTypeName(notNullValueAttributeTypeName);
      instances = function.GetAttributeInstances(notNullTypeName, true);
      if(instances == null || instances.Count == 0) {
        return true;
      }

      Rule rule = Rule.GetRule(type) ?? Rule.GetDefaultRule();
      if(rule == null) {
        return true;
      }

      if(string.IsNullOrEmpty(rule.ReturnAssertion)) {
        return true;
      }

      IInvocationExpression invocationExpression = returnStatement.Value as IInvocationExpression;
      if(invocationExpression != null) {
        IReferenceExpression invokedExpression = invocationExpression.InvokedExpression as IReferenceExpression;
        if(invokedExpression == null) {
          return false;
        }

        ResolveResult resolveResult = invokedExpression.Reference.Resolve();

        IMethod method = null;

        IMethodDeclaration methodDeclaration = resolveResult.DeclaredElement as IMethodDeclaration;
        if(methodDeclaration != null) {
          method = methodDeclaration as IMethod;
        }

        if(method == null) {
          method = resolveResult.DeclaredElement as IMethod;
        }

        if(method == null) {
          return false;
        }

        CodeAnnotationsCache codeAnnotationsCache = CodeAnnotationsCache.GetInstance(_solution);

        if (codeAnnotationsCache.IsAssertionMethod(method)) {
          return true;
        }
      }

      IFunctionDeclaration functionDeclaration = function as IFunctionDeclaration;

      ICSharpControlFlowGraf graf = CSharpControlFlowBuilder.Build(functionDeclaration);

      if (!graf.Inspect(true)) {
        return true;
      }

      IReferenceExpression referenceExpression = returnStatement.Value as IReferenceExpression;
      if(referenceExpression == null) {
        return true;
      }

      CSharpControlFlowNullReferenceState state = graf.GetExpressionNullReferenceState(referenceExpression);

      switch(state) {
        case CSharpControlFlowNullReferenceState.UNKNOWN:
          return false;
        case CSharpControlFlowNullReferenceState.NOT_NULL:
          return true;
        case CSharpControlFlowNullReferenceState.NULL:
          return false;
        case CSharpControlFlowNullReferenceState.MAY_BE_NULL:
          return false;
      }

      return true;
    }

    #endregion
  }
}