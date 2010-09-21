// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AfterInvocation.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The after invocation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.LiveTemplates
{
  using System.Collections.Generic;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp.Tree;

  /// <summary>The after invocation.</summary>
  [LiveTemplate("After invocation", "Executes a Live Template after the invocation of a method.")]
  public class AfterInvocation : ILiveTemplate
  {
    #region Implemented Interfaces

    #region ILiveTemplate

    /// <summary>Gets the name of the template.</summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The items.</returns>
    public IEnumerable<LiveTemplateItem> GetItems(SmartGenerateParameters parameters)
    {
      var previousStatement = parameters.PreviousStatement;

      var expressionStatement = previousStatement as IExpressionStatement;
      if (expressionStatement == null)
      {
        return null;
      }

      var invocationExpression = expressionStatement.Expression as IInvocationExpression;
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

      var shortName = method.ShortName;
      var type = string.Empty;

      var text = shortName;
      var shortcut = shortName;

      var containingType = method.GetContainingType();
      if (containingType != null)
      {
        text = containingType.ShortName + "." + text;
        shortcut = containingType.ShortName + "." + shortcut;
        type = containingType.ShortName;

        var ns = containingType.GetContainingNamespace();
        if (!string.IsNullOrEmpty(ns.ShortName))
        {
          shortcut = ns.ShortName + "." + shortcut;
        }
      }

      var liveTemplateItem = new LiveTemplateItem
      {
        MenuText = string.Format("After call to '{0}'", text),
        Description = string.Format("After call to '{0}'", text),
        Shortcut = string.Format("After call to {0}", shortcut),
        Text = string.Format("/* $Name$: method name, $Type$: containing type name*/\n")
      };

      liveTemplateItem.Variables["Name"] = shortName;
      liveTemplateItem.Variables["Type"] = type;

      return new List<LiveTemplateItem>
      {
        liveTemplateItem
      };
    }

    #endregion

    #endregion
  }
}