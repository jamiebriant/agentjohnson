// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AfterLocalVariableDeclaration.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The after local variable declaration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.LiveTemplates
{
  using System;
  using System.Collections.Generic;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp.Tree;

  /// <summary>The after local variable declaration.</summary>
  public class AfterLocalVariableDeclaration : ILiveTemplate
  {
    #region Implemented Interfaces

    #region ILiveTemplate

    /// <summary>Gets the name of the template.</summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The items.</returns>
    public IEnumerable<LiveTemplateItem> GetItems(SmartGenerateParameters parameters)
    {
      var element = parameters.Element;
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

      var value = localVariableDeclaration.Initial.GetText();

      var localVariable = localVariableDeclaration as ILocalVariable;
      if (localVariable == null)
      {
        return null;
      }

      var type = localVariable.Type;

      var presentableName = type.GetPresentableName(element.Language);
      var longPresentableName = type.GetLongPresentableName(element.Language);
      var shortName = localVariable.ShortName;

      var liveTemplateItem = new LiveTemplateItem
      {
        MenuText = string.Format("After local variable declaration of type '{0}'", presentableName), 
        Description = string.Format("After local variable of type '{0}'", presentableName), 
        Shortcut = string.Format("After local variable of type {0}", longPresentableName),
        Text = string.Format("/* @VariableName, @VariableType, @Value */\n")
      };

      liveTemplateItem.Variables["VariableName"] = shortName;
      liveTemplateItem.Variables["VariableType"] = presentableName;
      liveTemplateItem.Variables["Value"] = value;

      return new List<LiveTemplateItem>
      {
        liveTemplateItem
      };
    }

    #endregion

    #endregion
  }
}