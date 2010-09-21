// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LiveTemplate.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The live template.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.Generators
{
  using System;
  using System.Reflection;
  using System.Xml;
  using JetBrains.ReSharper.Feature.Services.LiveTemplates.LiveTemplates;
  using JetBrains.ReSharper.Feature.Services.LiveTemplates.Storages;
  using JetBrains.Util;

  /// <summary>The live template.</summary>
  [SmartGenerate("Execute Live Template", "Executes a Live Template.", Priority = -10)]
  public class LiveTemplate : SmartGenerateHandlerBase
  {
    #region Methods

    /// <summary>Gets the items.</summary>
    /// <param name="smartGenerateParameters">The get menu items parameters.</param>
    protected override void GetItems(SmartGenerateParameters smartGenerateParameters)
    {
      var element = smartGenerateParameters.Element;

      smartGenerateParameters.PreviousStatement = StatementUtil.GetPreviousStatement(element);

      var defaultRange = StatementUtil.GetNewStatementPosition(element);

      foreach (var liveTemplateInfo in LiveTemplateManager.Instance.LiveTemplateInfos)
      {
        var constructor = liveTemplateInfo.Type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);

        var handler = constructor != null ? (ILiveTemplate)constructor.Invoke(new object[]
        {
        }) : (ILiveTemplate)Activator.CreateInstance(liveTemplateInfo.Type);
        if (handler == null)
        {
          continue;
        }

        var liveTemplateItems = handler.GetItems(smartGenerateParameters);
        if (liveTemplateItems == null)
        {
          continue;
        }

        foreach (var liveTemplateMenuItem in liveTemplateItems)
        {
          var shortcut = "`Do not change: " + liveTemplateMenuItem.Shortcut;

          foreach (var templateStorage in LiveTemplatesManager.Instance.TemplateFamily.TemplateStorages)
          {
            this.GetItems(liveTemplateMenuItem, templateStorage, shortcut, defaultRange);
          }
        }
      }

      if (this.HasItems)
      {
        this.AddMenuSeparator();
      }
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="liveTemplateMenuItem">The live template menu item.</param>
    /// <param name="templateStorage">The template storage.</param>
    /// <param name="shortcut">The shortcut.</param>
    /// <param name="defaultRange">The default range.</param>
    private void GetItems(LiveTemplateItem liveTemplateMenuItem, ITemplateStorage templateStorage, string shortcut, TextRange defaultRange)
    {
      foreach (var template in templateStorage.Templates)
      {
        if (template.Shortcut != shortcut)
        {
          continue;
        }

        var doc = new XmlDocument();

        var templateElement = template.WriteToXml(doc);

        var xml = templateElement.OuterXml;
        var description = template.Description;

        foreach (var key in liveTemplateMenuItem.Variables.Keys)
        {
          xml = xml.Replace("@" + key, liveTemplateMenuItem.Variables[key].Replace("\"", "&quot;"));
          description = description.Replace("@" + key, liveTemplateMenuItem.Variables[key]);
        }

        var range = liveTemplateMenuItem.Range;
        if (range == TextRange.InvalidRange)
        {
          range = defaultRange;
        }

        this.AddAction(description, xml, range);
      }
    }

    #endregion
  }
}