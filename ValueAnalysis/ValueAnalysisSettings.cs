using System.Collections.Generic;
using System.IO;
using System.Xml;
using JetBrains.Application;
using JetBrains.ComponentModel;
using JetBrains.Util;

namespace AgentJohnson.ValueAnalysis {
  /// <summary>
  /// 
  /// </summary>
  [ShellComponentInterface(ProgramConfigurations.VS_ADDIN)]
  [ShellComponentImplementation]
  public class ValueAnalysisSettings : IXmlExternalizableShellComponent {
    #region Fields

    string _allowNullAttribute;
    List<Rule> _rules = new List<Rule>();
    bool _executeGhostDoc;

    #endregion

    #region Public properties

    /// <summary>
    /// Gets or sets the allow null attribute.
    /// </summary>
    /// <value>The allow null attribute.</value>
    [XmlExternalizable("")]
    public string AllowNullAttribute {
      get {
        return _allowNullAttribute ?? string.Empty;
      }
      set {
        _allowNullAttribute = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether GhostDoc should be executed.
    /// </summary>
    /// <value><c>true</c> if GhostDoc should be executed; otherwise, <c>false</c>.</value>
    [XmlExternalizable(false)]
    public bool ExecuteGhostDoc {
      get {
        return _executeGhostDoc;
      }
      set {
        _executeGhostDoc = value;
      }
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static ValueAnalysisSettings Instance {
      get {
        return Shell.Instance.GetComponent<ValueAnalysisSettings>();
      }
    }

    /// <summary>
    /// Gets or sets the type assertions.
    /// </summary>
    /// <remarks>This is for serialization only.</remarks>
    /// <value>The type assertions.</value>
    [XmlExternalizable("")]
    public string SerializableTypeConfigurations {
      get {
        StringWriter stringWriter = new StringWriter();

        XmlTextWriter writer = new XmlTextWriter(stringWriter);

        Write(writer);

        return stringWriter.ToString();
      }
      set {
        _rules = new List<Rule>();

        if(string.IsNullOrEmpty(value)) {
          return;
        }

        XmlDocument doc = new XmlDocument();

        doc.LoadXml(value);

        XmlNodeList nodes = doc.SelectNodes("/types/type");
        if(nodes == null) {
          return;
        }

        Read(nodes);
      }
    }

    /// <summary>
    /// Gets or sets the rules.
    /// </summary>
    /// <value>The rules.</value>
    public List<Rule> Rules {
      get {
        return _rules;
      }
      set {
        _rules = value;
      }
    }

    #endregion

    #region IShellComponent implementation

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Init() {
    }

    #endregion

    #region IXmlExternalizableShellComponent implementation

    #region Public methods

    /// <summary>
    /// This method must not fail with null or unexpected Xml!!!
    /// </summary>
    /// <param name="element"></param>
    public void ReadFromXml(XmlElement element) {
      if(element == null) {
        return;
      }

      XmlExternalizationUtil.ReadFromXml(element, this);
    }

    /// <summary>
    /// Writes to XML.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public void WriteToXml(XmlElement element) {
      XmlExternalizationUtil.WriteToXml(element, this);
    }

    #endregion

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    /// <value>The name of the tag.</value>
    public string TagName {
      get {
        return "AgentJohnson";
      }
    }

    /// <summary>
    /// Scope that defines which store the data goes into.
    /// Must not be
    /// <c>0</c>.
    /// </summary>
    /// <value></value>
    public XmlExternalizationScope Scope {
      get {
        return XmlExternalizationScope.UserSettings;
      }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Writes the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    public void WriteSettings(XmlTextWriter writer) {
      writer.WriteStartElement("valueanalysis");

      Write(writer);

      writer.WriteElementString("allownull", AllowNullAttribute);
      writer.WriteElementString("executeghostdoc", ExecuteGhostDoc ? "true" : "false");

      writer.WriteEndElement();
    }

    /// <summary>
    /// Reads the settings.
    /// </summary>
    /// <param name="doc">The document.</param>
    public void ReadSettings(XmlDocument doc) {
      _rules.Clear();
      AllowNullAttribute = string.Empty;

      XmlNodeList nodes = doc.SelectNodes("/settings/valueanalysis/types/type");

      Read(nodes);

      XmlNode allowNullNode = doc.SelectSingleNode("/settings/valueanalysis/allownull");
      if(allowNullNode != null) {
        AllowNullAttribute = allowNullNode.InnerText;
      }

      XmlNode executeGhostDocNode = doc.SelectSingleNode("/settings/valueanalysis/executeghostdoc");
      if(executeGhostDocNode != null) {
        ExecuteGhostDoc = executeGhostDocNode.InnerText == "true";
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Gets the attribute.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="name">The name.</param>
    /// <returns>The attribute.</returns>
    static string GetAttributeString(XmlNode node, string name) {
      XmlAttribute attribute = node.Attributes[name];

      return attribute == null ? string.Empty : (attribute.Value ?? string.Empty);
    }

    /// <summary>
    /// Gets the element.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="name">The name.</param>
    /// <returns>The element.</returns>
    static string GetElementString(XmlNode node, string name) {
      XmlNode element = node.SelectSingleNode(name);

      return element == null ? string.Empty : element.InnerText;
    }

    /// <summary>
    /// Reads the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    void Read(XmlNodeList nodes) {
      foreach(XmlNode type in nodes) {
        Rule rule = new Rule
        {
          TypeName = GetAttributeString(type, "type"),
          NotNull = GetAttributeString(type, "notnull") == "true",
          CanBeNull = GetAttributeString(type, "canbenull") == "true",
          PublicParameterAssertion = GetElementString(type, "publicparameterassertion"),
          NonPublicParameterAssertion = GetElementString(type, "nonpublicparameterassertion"),
          ReturnAssertion = GetElementString(type, "returnassertion")
        };

        XmlNodeList valueAssertions = type.SelectNodes("valueassertions/valueassertion");
        if(valueAssertions != null) {
          foreach(XmlNode valueAssertion in valueAssertions) {
            rule.ValueAssertions.Add(valueAssertion.InnerText);
          }
        }

        _rules.Add(rule);
      }
    }

    /// <summary>
    /// Writes the settings.
    /// </summary>
    /// <param name="writer">The writer.</param>
    void Write(XmlTextWriter writer) {
      writer.WriteStartElement("types");

      foreach(Rule configuration in Rules) {
        writer.WriteStartElement("type");

        writer.WriteAttributeString("type", configuration.TypeName);
        writer.WriteAttributeString("notnull", configuration.NotNull ? "true" : "false");
        writer.WriteAttributeString("canbenull", configuration.CanBeNull ? "true" : "false");
        writer.WriteElementString("publicparameterassertion", configuration.PublicParameterAssertion);
        writer.WriteElementString("nonpublicparameterassertion", configuration.NonPublicParameterAssertion);
        writer.WriteElementString("returnassertion", configuration.ReturnAssertion);

        writer.WriteStartElement("valueassertions");

        foreach(string assertion in configuration.ValueAssertions) {
          writer.WriteElementString("valueassertion", assertion);
        }

        writer.WriteEndElement();

        writer.WriteEndElement();
      }

      writer.WriteEndElement();
    }

    #endregion
  }
}
