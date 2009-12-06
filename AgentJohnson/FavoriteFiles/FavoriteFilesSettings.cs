using System.Collections.Generic;
using System.Text;
using System.Xml;
using JetBrains.Application;
using JetBrains.ComponentModel;

namespace AgentJohnson.FavoriteFiles {
  /// <summary>
  /// Represents the Favorite Files Settings.
  /// </summary>
  [ShellComponentInterface(ProgramConfigurations.VS_ADDIN)]
  [ShellComponentImplementation]
  public class FavoriteFilesSettings : IXmlExternalizableShellComponent {
    #region Fields

    List<FavoriteFilePath> favoriteFiles;

    #endregion

    #region Public properties

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static FavoriteFilesSettings Instance {
      get {
        return Shell.Instance.GetComponent<FavoriteFilesSettings>();
      }
    }

    /// <summary>
    /// Gets or sets the favorite files.
    /// </summary>
    /// <value>The favorite files.</value>
    public List<FavoriteFilePath> FavoriteFiles {
      get {
        if(this.favoriteFiles == null){
          InitFavoriteFiles();
        }

        return this.favoriteFiles;
      }
      set {
        this.favoriteFiles = value;
      }
    }
    /// <summary>
    /// Gets or sets the files in a serializable format.
    /// </summary>                             
    /// <remarks>This is for serialization only.</remarks>
    /// <value>The files.</value>
    [global::JetBrains.Util.XmlExternalizable(true)]
    public string SerializableFavoriteFiles {
      get {
        StringBuilder result = new StringBuilder();

        bool first = true;

        foreach(FavoriteFilePath path in FavoriteFiles){
          if(!first){
            result.Append("|");
          }

          first = false;

          result.Append(path.ToString());
        }

        return result.ToString();
      }
      set {
        if (value == null) {
          return;
        }

        this.favoriteFiles = new List<FavoriteFilePath>();

        string[] files = value.Split('|');

        foreach(string favoriteFilePath in files){
          if(string.IsNullOrEmpty(favoriteFilePath)){
            continue;
          }

          FavoriteFilePath path = new FavoriteFilePath(favoriteFilePath);

          this.favoriteFiles.Add(path);
        }
      }
    }

    #endregion

    #region IShellComponent implementation

    /// <summary>
    /// Inits this instance.
    /// </summary>
    public void Init() {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
    }

    #endregion

    #region IXmlExternalizableShellComponent implementation

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    /// <value>The name of the tag.</value>
    public string TagName {
      get {
        return "ReSharper.PowerToys.FavoriteFiles";
      }
    }

    ///<summary>
    ///
    ///            Scope that defines which store the data goes into.
    ///            Must not be 
    ///<c>0</c>.
    ///            
    ///</summary>
    ///
    public XmlExternalizationScope Scope {
      get {
        return XmlExternalizationScope.UserSettings;
      }
    }

    /// <summary>
    /// This method must not fail with null or unexpected Xml!!!
    /// </summary>
    /// <param name="element"></param>
    public void ReadFromXml(XmlElement element) {
      if (element == null){
        InitFavoriteFiles();
        return;
      }

      global::JetBrains.Util.XmlExternalizationUtil.ReadFromXml(element, this);
    }

    /// <summary>
    /// Writes to XML.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public void WriteToXml(XmlElement element) {
      global::JetBrains.Util.XmlExternalizationUtil.WriteToXml(element, this);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Inits the files.
    /// </summary>
    void InitFavoriteFiles() {
      this.favoriteFiles = new List<FavoriteFilePath>();
    }

    #endregion
  }
}