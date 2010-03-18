// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueAnalysisDescriptor.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the DocumentationDescriptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.CodeCleanup.Descriptors
{
  #region Using Directives

  using System;
  using System.ComponentModel;
  using System.Xml;
  using AgentJohnson.CodeCleanup.Options;
  using JetBrains.Annotations;
  using JetBrains.ReSharper.Feature.Services.CodeCleanup;
  using JetBrains.Util;

  #endregion

  /// <summary>Code Clean Up Description.</summary>
  [Category("Agent Johnson")]
  [DisplayName("Value Analysis")]
  [TypeConverter(typeof(ExpandableObjectConverter))]
  [UsedImplicitly]
  public class ValueAnalysisDescriptor : CodeCleanupOptionDescriptor<ValueAnalysisOptions>
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueAnalysisDescriptor"/> class. 
    /// </summary>
    public ValueAnalysisDescriptor() : base("AgentJohnsonValueAnalysis")
    {
    }

    #endregion

    #region Public Methods

    /// <summary>Loads the specified profile.</summary>
    /// <param name="profile">The profile.</param>
    /// <param name="element">The element.</param>
    public override void Load(CodeCleanupProfile profile, XmlElement element)
    {
      var options = new ValueAnalysisOptions();
      var optionsElement = (XmlElement)element.SelectSingleNode(this.Name);

      if (optionsElement != null)
      {
        try
        {
          options.AnnotateWithValueAnalysisAttributes = bool.Parse(XmlUtil.ReadLeafElementValue(optionsElement, "ValueAnalysis"));
          options.InsertAssertStatements = bool.Parse(XmlUtil.ReadLeafElementValue(optionsElement, "InsertStatements"));
        }
        catch (ArgumentException)
        {
        }
      }

      profile.SetSetting(this, options);
    }

    /// <summary>Presents the specified profile.</summary>
    /// <param name="profile">The profile.</param>
    /// <returns>Specified profile.</returns>
    public override string Present(CodeCleanupProfile profile)
    {
      return profile.GetSetting(this).ToString();
    }

    /// <summary>Saves the specified profile.</summary>
    /// <param name="profile">The profile.</param>
    /// <param name="element">The element.</param>
    public override void Save(CodeCleanupProfile profile, XmlElement element)
    {
      var options = profile.GetSetting(this);
      var optionsElement = element.CreateElement(this.Name);

      optionsElement.CreateLeafElementWithValue("ValueAnalysis", options.AnnotateWithValueAnalysisAttributes.ToString());
      optionsElement.CreateLeafElementWithValue("InsertStatements", options.InsertAssertStatements.ToString());
    }

    #endregion
  }
}