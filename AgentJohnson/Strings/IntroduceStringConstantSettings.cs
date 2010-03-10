// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntroduceStringConstantSettings.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Favorite Files Settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.Strings
{
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;
  using JetBrains.Application;
  using JetBrains.ComponentModel;
  using JetBrains.Util;

  /// <summary>Represents the Favorite Files Settings.</summary>
  [ShellComponentInterface(ProgramConfigurations.VS_ADDIN)]
  [ShellComponentImplementation]
  public class IntroduceStringConstantSettings : IXmlExternalizableShellComponent
  {
    #region Constants and Fields

    /// <summary>The _class names.</summary>
    private List<string> _classNames;

    /// <summary>The _generate xml comment.</summary>
    private bool _generateXmlComment;

    /// <summary>The _replace spaces mode.</summary>
    private int _replaceSpacesMode;

    #endregion

    #region Implemented Interfaces

    #region IXmlExternalizableComponent

    ///<summary>
    ///
    ///            Scope that defines which store the data goes into.
    ///            Must not be 
    ///<c>0</c>.
    ///            
    ///</summary>
    ///
    public XmlExternalizationScope Scope
    {
      get
      {
        return XmlExternalizationScope.WorkspaceSettings;
      }
    }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    /// <value>The name of the tag.</value>
    string IXmlExternalizableComponent.TagName
    {
      get
      {
        return "AgentJohnson.IntroduceStringConstant";
      }
    }

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static IntroduceStringConstantSettings Instance
    {
      get
      {
        return Shell.Instance.GetComponent<IntroduceStringConstantSettings>();
      }
    }

    /// <summary>
    /// Gets or sets the class names.
    /// </summary>
    /// <value>The class names.</value>
    public List<string> ClassNames
    {
      get
      {
        if (this._classNames == null)
        {
          this.InitClasses();
        }

        return this._classNames;
      }

      set
      {
        this._classNames = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [generate XML comment].
    /// </summary>
    /// <value><c>true</c> if [generate XML comment]; otherwise, <c>false</c>.</value>
    [XmlExternalizable(true)]
    public bool GenerateXmlComment
    {
      get
      {
        return this._generateXmlComment;
      }

      set
      {
        this._generateXmlComment = value;
      }
    }

    /// <summary>
    /// Gets or sets the replace spaces mode.
    /// </summary>
    /// <value>The replace spaces mode.</value>
    [XmlExternalizable(0)]
    public int ReplaceSpacesMode
    {
      get
      {
        return this._replaceSpacesMode;
      }

      set
      {
        this._replaceSpacesMode = value;
      }
    }

    /// <summary>
    /// Gets or sets the class names in a serializable format.
    /// </summary>                             
    /// <remarks>This is for serialization only.</remarks>
    /// <value>The class names.</value>
    [XmlExternalizable("")]
    public string SerializableClassNames
    {
      get
      {
        var result = new StringBuilder();

        var first = true;

        foreach (var className in this.ClassNames)
        {
          if (!first)
          {
            result.Append("|");
          }

          first = false;

          result.Append(className);
        }

        return result.ToString();
      }

      set
      {
        this._classNames = new List<string>();

        if (string.IsNullOrEmpty(value))
        {
          return;
        }

        var classes = value.Split('|');

        foreach (var className in classes)
        {
          if (!string.IsNullOrEmpty(className))
          {
            this._classNames.Add(className);
          }
        }
      }
    }

    #endregion

    #region Public Methods

    /// <summary>Reads the settings.</summary>
    /// <param name="doc">The doc.</param>
    public void ReadSettings(XmlDocument doc)
    {
      var node = doc.SelectSingleNode("/settings/introducestringconstant");
      if (node == null)
      {
        return;
      }

      this.GenerateXmlComment = GetAttributeString(node, "generatexmlcomment") == "true";
      this.ReplaceSpacesMode = int.Parse(GetAttributeString(node, "replacespacesmode"));

      this._classNames.Clear();

      var classes = node.SelectNodes("class");
      if (classes == null)
      {
        return;
      }

      foreach (XmlNode xmlNode in classes)
      {
        this._classNames.Add(xmlNode.InnerText);
      }
    }

    /// <summary>Writes the settings.</summary>
    /// <param name="writer">The writer.</param>
    public void WriteSettings(XmlTextWriter writer)
    {
      writer.WriteStartElement("introducestringconstant");

      writer.WriteAttributeString("generatexmlcomment", this.GenerateXmlComment ? "true" : "false");

      writer.WriteAttributeString("replacespacesmode", this.ReplaceSpacesMode.ToString());

      foreach (var className in this.ClassNames)
      {
        writer.WriteElementString("class", className);
      }

      writer.WriteEndElement();
    }

    #endregion

    #region Implemented Interfaces

    #region IComponent

    /// <summary>Inits this instance.</summary>
    public void Init()
    {
    }

    #endregion

    #region IDisposable

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
    }

    #endregion

    #region IXmlExternalizable

    /// <summary>This method must not fail with null or unexpected Xml!!!</summary>
    /// <param name="element"></param>
    void IXmlExternalizable.ReadFromXml(XmlElement element)
    {
      if (element == null)
      {
        this.InitClasses();
        return;
      }

      XmlExternalizationUtil.ReadFromXml(element, this);
    }

    /// <summary>Writes to XML.</summary>
    /// <param name="element">The element.</param>
    void IXmlExternalizable.WriteToXml(XmlElement element)
    {
      XmlExternalizationUtil.WriteToXml(element, this);
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>Gets the attribute.</summary>
    /// <param name="node">The node.</param>
    /// <param name="name">The name.</param>
    /// <returns>The attribute.</returns>
    private static string GetAttributeString(XmlNode node, string name)
    {
      var attribute = node.Attributes[name];

      return attribute == null ? string.Empty : (attribute.Value ?? string.Empty);
    }

    /// <summary>Inits the files.</summary>
    private void InitClasses()
    {
      this._classNames = new List<string>();
    }

    #endregion
  }
}