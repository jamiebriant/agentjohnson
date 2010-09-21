// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AfterLocalVariableInitializer.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The after local variable initializer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.LiveTemplates
{
  using System.Collections.Generic;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp.Tree;

  /// <summary>The after local variable initializer.</summary>
  [LiveTemplate("After initialization with call to a method", "Executes a Live Template after initialization with call to a method.")]
  public class AfterLocalVariableInitializer : ILiveTemplate
  {
    #region Implemented Interfaces

    #region ILiveTemplate

    /// <summary>Gets the name of the template.</summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The items.</returns>
    public IEnumerable<LiveTemplateItem> GetItems(SmartGenerateParameters parameters)
    {
      var previousStatement = parameters.PreviousStatement;

      var declarationStatement = previousStatement as IDeclarationStatement;
      if (declarationStatement == null)
      {
        return null;
      }

      var localVariableDeclarations = declarationStatement.VariableDeclarations;
      if (localVariableDeclarations == null || localVariableDeclarations.Count != 1)
      {
        return null;
      }

      var localVariableDeclaration = localVariableDeclarations[0];
      if (localVariableDeclaration == null)
      {
        return null;
      }

      var localVariable = localVariableDeclaration.DeclaredElement as ILocalVariable;
      if (localVariable == null)
      {
        return null;
      }

      var initial = localVariableDeclaration.Initial as IExpressionInitializer;
      if (initial == null)
      {
        return null;
      }

      var invocationExpression = initial.Value as IInvocationExpression;
      if (invocationExpression == null)
      {
        return null;
      }

      var invokedExpression = invocationExpression.InvokedExpression as IReferenceExpression;
      if (invokedExpression == null)
      {
        return null;
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
        return null;
      }

      var variableName = localVariable.ShortName;
      var shortName = method.ShortName;
      var typeName = string.Empty;

      var text = shortName;
      var shortcut = shortName;

      var containingType = method.GetContainingType();
      if (containingType != null)
      {
        text = containingType.ShortName + "." + text;
        shortcut = containingType.ShortName + "." + shortcut;
        typeName = containingType.ShortName;

        var ns = containingType.GetContainingNamespace();
        if (!string.IsNullOrEmpty(ns.ShortName))
        {
          shortcut = ns.ShortName + "." + shortcut;
        }
      }

      var liveTemplateItem = new LiveTemplateItem
      {
        MenuText = string.Format("After initialization with call to '{0}'", text), 
        Description = string.Format("After initialization with call to '{0}'", text), 
        Shortcut = string.Format("After initialization with call to {0}", shortcut),
        Text = string.Format("/* $Variable$: variable name, $Name$ = method name, $Type$ = variable type name */\n")
      };

      liveTemplateItem.Variables["Variable"] = variableName;
      liveTemplateItem.Variables["Name"] = shortName;
      liveTemplateItem.Variables["Type"] = typeName;

      return new List<LiveTemplateItem>
      {
        liveTemplateItem
      };
    }

    #endregion

    #endregion
  }
}