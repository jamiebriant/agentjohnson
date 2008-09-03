﻿using System.Collections.Generic;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace AgentJohnson.SmartGenerate.LiveTemplates {
  /// <summary>
  /// </summary>
  [LiveTemplate("Class member", "Generate class member")]
  public class ClassMembers : ILiveTemplate {
    #region Public methods

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The items.</returns>
    public IEnumerable<LiveTemplateItem> GetItems(SmartGenerateParameters parameters) {
      IElement element = parameters.Element;

      IClassDeclaration classDeclaration = element.GetContainingElement(typeof(IClassDeclaration), true) as IClassDeclaration;
      if(classDeclaration == null) {
        return null;
      }

      IElement memberDeclaration = element.GetContainingElement(typeof(IClassMemberDeclaration), true);
      if(memberDeclaration != null && !(memberDeclaration is IClassDeclaration)) {
        return null;
      }

      string modifier = ModifierUtil.GetModifier(element, classDeclaration);

      LiveTemplateItem liveTemplateItem = new LiveTemplateItem {
        MenuText = "Class member",
        Description = "Class member",
        Shortcut = "Class member"
      };

      liveTemplateItem.Variables["Modifier"] = modifier;

      return new List<LiveTemplateItem> {
        liveTemplateItem
      };
    }

    #endregion
  }
}