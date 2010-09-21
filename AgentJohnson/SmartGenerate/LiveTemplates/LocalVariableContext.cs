// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalVariableContext.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The local variable context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.LiveTemplates
{
  using System.Collections.Generic;

  /// <summary>The local variable context.</summary>
  [LiveTemplate("Local Variable Context", "Generate local Variable Context")]
  public class LocalVariableContext : ILiveTemplate
  {
    #region Implemented Interfaces

    #region ILiveTemplate

    /// <summary>Gets the name of the template.</summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The items.</returns>
    IEnumerable<LiveTemplateItem> ILiveTemplate.GetItems(SmartGenerateParameters parameters)
    {
      var element = parameters.Element;

      var scope = parameters.Scope;
      if (scope.Count == 0)
      {
        return null;
      }

      var name = scope[parameters.ScopeIndex].Name;
      var type = scope[parameters.ScopeIndex].Type;

      var result = new List<LiveTemplateItem>();

      var presentableName = type.GetPresentableName(element.Language);

      var liveTemplateItem = new LiveTemplateItem
      {
        MenuText = string.Format("After declaration of variable of type '{0}'", presentableName), 
        Description = string.Format("After declaration of variable of type '{0}'", presentableName), 
        Shortcut = string.Format("After declaration of variable of type '{0}'", type.GetLongPresentableName(element.Language)),
        Text = string.Format("/* @VariableName, @VariableType */\n")
      };

      liveTemplateItem.Variables["VariableName"] = name;
      liveTemplateItem.Variables["VariableType"] = presentableName;

      result.Add(liveTemplateItem);

      return result;
    }

    #endregion

    #endregion
  }
}