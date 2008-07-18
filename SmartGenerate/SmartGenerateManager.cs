﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.ComponentModel;

namespace AgentJohnson.SmartGenerate {

  /// <summary>
  /// 
  /// </summary>
  internal class SmartGenerateInfo {
    public int Priority { get; set; }
    public string Name { get; set; }
    public ISmartGenerate Handler { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Returns the fully qualified type name of this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> containing a fully qualified type name.
    /// </returns>
    public override string ToString() {
      return Name;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  [ShellComponentImplementation(ProgramConfigurations.VS_ADDIN)]
  [ShellComponentInterface(ProgramConfigurations.ALL)]
  public class SmartGenerateManager : ITypeLoadingHandler, IShellComponent, IComparer<SmartGenerateInfo> {
    #region Fields

    List<SmartGenerateInfo> _handlers = new List<SmartGenerateInfo>();
    XmlDocument _templates;

    #endregion

    #region Public properties

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static SmartGenerateManager Instance {
      get {
        return (SmartGenerateManager)Shell.Instance.GetComponent(typeof(SmartGenerateManager));
      }
    }

    /// <summary>
    /// Gets the handlers.
    /// </summary>
    /// <value>The handlers.</value>
    internal List<SmartGenerateInfo> Handlers {
      get {
        return _handlers;
      }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
    /// </returns>
    int IComparer<SmartGenerateInfo>.Compare(SmartGenerateInfo x, SmartGenerateInfo y) {
      return x.Priority - y.Priority;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      _handlers = new List<SmartGenerateInfo>();
    }

    /// <summary>
    /// Gets the handlers.
    /// </summary>
    /// <returns>The handlers.</returns>
    [NotNull]
    public IEnumerable<ISmartGenerate> GetHandlers() {
      List<ISmartGenerate> result = new List<ISmartGenerate>();

      List<string> disabledHandlers = new List<string>(SmartGenerateSettings.Instance.DisabledActions.Split('|'));

      foreach(SmartGenerateInfo handler in _handlers) {
        if (disabledHandlers.Contains(handler.Name)) {
          continue;
        }

        result.Add(handler.Handler);
      }

      return result;
    }

    /// <summary>
    /// Gets the template.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns>The template.</returns>
    [CanBeNull]
    public string GetTemplate([NotNull] string template) {
      if(_templates == null) {
        string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\SmartGenerate.xml";

        if(!File.Exists(filename)) {
          return null;
        }

        _templates = new XmlDocument();

        _templates.Load(filename);
      }

      XmlNode node = _templates.SelectSingleNode(string.Format("/*/Template[@uid='{0}' or shortcut='{0}']", template));
      if(node == null) {
        return null;
      }

      return node.OuterXml;
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Init() {
      Shell.Instance.RegisterTypeLoadingHandler(this);
    }

    /// <summary>
    /// Called when types have been loaded.
    /// </summary>
    /// <param name="assemblies">The assemblies.</param>
    /// <param name="types">The types.</param>
    public void TypesLoaded(ICollection<Assembly> assemblies, ICollection<Type> types) {
      foreach(Type type in types) {
        object[] attributes = type.GetCustomAttributes(typeof(SmartGenerateAttribute), false);
        if(attributes.Length != 1) {
          continue;
        }

        SmartGenerateAttribute smartGenerateAttribute = (SmartGenerateAttribute)attributes[0];

        ConstructorInfo constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);

        ISmartGenerate handler = constructor != null ? (ISmartGenerate)constructor.Invoke(new object[] {}) : (ISmartGenerate)Activator.CreateInstance(type);
        if(handler == null) {
          continue;
        }

        SmartGenerateInfo entry = new SmartGenerateInfo {Priority = smartGenerateAttribute.Priority, Name = smartGenerateAttribute.Name, Description = smartGenerateAttribute.Description, Handler = handler};

        _handlers.Add(entry);
      }

      _handlers.Sort(this);
    }

    /// <summary>
    /// Called when types have been loaded.
    /// </summary>
    /// <param name="assemblies">The assemblies.</param>
    /// <param name="types">The types.</param>
    public void TypesUnloaded(ICollection<Assembly> assemblies, ICollection<Type> types) {
    }

    #endregion

    #region ITypeLoadingHandler Members

    /// <summary>
    /// Gets the attribute types.
    /// </summary>
    /// <value>The attribute types.</value>
    public Type[] AttributeTypes {
      get {
        return new[] {typeof(SmartGenerateAttribute)};
      }
    }

    #endregion
  }
}