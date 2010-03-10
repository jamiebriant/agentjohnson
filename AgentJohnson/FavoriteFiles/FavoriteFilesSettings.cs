// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FavoriteFilesSettings.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Favorite Files Settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.FavoriteFiles
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
  public class FavoriteFilesSettings : IXmlExternalizableShellComponent
  {
    #region Constants and Fields

    /// <summary>The favorite files.</summary>
    private List<FavoriteFilePath> favoriteFiles;

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
        return XmlExternalizationScope.UserSettings;
      }
    }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    /// <value>The name of the tag.</value>
    public string TagName
    {
      get
      {
        return "ReSharper.PowerToys.FavoriteFiles";
      }
    }

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static FavoriteFilesSettings Instance
    {
      get
      {
        return Shell.Instance.GetComponent<FavoriteFilesSettings>();
      }
    }

    /// <summary>
    /// Gets or sets the favorite files.
    /// </summary>
    /// <value>The favorite files.</value>
    public List<FavoriteFilePath> FavoriteFiles
    {
      get
      {
        if (this.favoriteFiles == null)
        {
          this.InitFavoriteFiles();
        }

        return this.favoriteFiles;
      }

      set
      {
        this.favoriteFiles = value;
      }
    }

    /// <summary>
    /// Gets or sets the files in a serializable format.
    /// </summary>                             
    /// <remarks>This is for serialization only.</remarks>
    /// <value>The files.</value>
    [XmlExternalizable(true)]
    public string SerializableFavoriteFiles
    {
      get
      {
        var result = new StringBuilder();

        var first = true;

        foreach (var path in this.FavoriteFiles)
        {
          if (!first)
          {
            result.Append("|");
          }

          first = false;

          result.Append(path.ToString());
        }

        return result.ToString();
      }

      set
      {
        if (value == null)
        {
          return;
        }

        this.favoriteFiles = new List<FavoriteFilePath>();

        var files = value.Split('|');

        foreach (var favoriteFilePath in files)
        {
          if (string.IsNullOrEmpty(favoriteFilePath))
          {
            continue;
          }

          var path = new FavoriteFilePath(favoriteFilePath);

          this.favoriteFiles.Add(path);
        }
      }
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
    public void ReadFromXml(XmlElement element)
    {
      if (element == null)
      {
        this.InitFavoriteFiles();
        return;
      }

      XmlExternalizationUtil.ReadFromXml(element, this);
    }

    /// <summary>Writes to XML.</summary>
    /// <param name="element">The element.</param>
    public void WriteToXml(XmlElement element)
    {
      XmlExternalizationUtil.WriteToXml(element, this);
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>Inits the files.</summary>
    private void InitFavoriteFiles()
    {
      this.favoriteFiles = new List<FavoriteFilePath>();
    }

    #endregion
  }
}