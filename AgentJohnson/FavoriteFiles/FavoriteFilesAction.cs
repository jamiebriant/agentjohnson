// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FavoriteFilesAction.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Handles Find Text action, see Actions.xml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.FavoriteFiles
{
  using System.Collections.Generic;
  using System.Drawing;
  using System.IO;
  using System.Windows.Forms;
  using EnvDTE;
  using JetBrains.ActionManagement;
  using JetBrains.CommonControls;
  using JetBrains.DocumentModel;
  using JetBrains.IDE;
  using JetBrains.ProjectModel;
  using JetBrains.UI.PopupMenu;
  using JetBrains.VsIntegration.Application;

  /// <summary>
  /// Handles Find Text action, see Actions.xml
  /// </summary>
  [ActionHandler("FavoriteFiles")]
  public class FavoriteFilesAction : IActionHandler
  {
    #region Constants and Fields

    /// <summary>
    /// The _current file.
    /// </summary>
    private FavoriteFilePath currentFile;

    /// <summary>
    /// The _solution.
    /// </summary>
    private ISolution solution;

    #endregion

    #region Implemented Interfaces

    #region IActionHandler

    /// <summary>
    /// Executes action. Called after Update, that set <see cref="ActionPresentation"/>.Enabled to true.
    /// </summary>
    /// <param name="context">
    /// <c>DataContext</c>
    /// </param>
    /// <param name="nextExecute">
    /// delegate to call
    /// </param>
    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var solution = context.GetData(DataConstants.SOLUTION);
      if (solution == null)
      {
        return;
      }

      Execute(solution, context);
    }

    /// <summary>
    /// Updates action visual presentation. If presentation.Enabled is set to false, Execute
    /// will not be called.
    /// </summary>
    /// <param name="context">
    /// <c>DataContext</c>
    /// </param>
    /// <param name="presentation">
    /// presentation to update
    /// </param>
    /// <param name="nextUpdate">
    /// delegate to call
    /// </param>
    /// <returns>
    /// The update.
    /// </returns>
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return context.CheckAllNotNull(DataConstants.SOLUTION);
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>
    /// Describes the favorite file.
    /// </summary>
    /// <param name="favoriteFilePath">
    /// The favorite file path.
    /// </param>
    /// <param name="index">
    /// The index.
    /// </param>
    /// <returns>
    /// The favorite file.
    /// </returns>
    private static SimpleMenuItem DescribeFavoriteFile(FavoriteFilePath favoriteFilePath, int index)
    {
      var result = new SimpleMenuItem();

      if (favoriteFilePath == null)
      {
        result.Text = "<Error>";
        return result;
      }

      try
      {
        result.Text = Path.GetFileName(favoriteFilePath.Path);
      }
      catch
      {
        result.Text = favoriteFilePath.Path;
      }

      result.Style = MenuItemStyle.Enabled;
      result.ShortcutText = new global::JetBrains.UI.RichText.RichText("(" + favoriteFilePath + ")", global::JetBrains.UI.RichText.TextStyle.FromForeColor(Color.LightGray));

      if (index < 0 || index > 8)
      {
        return result;
      }

      result.Mnemonic = index.ToString();

      return result;
    }

    /// <summary>
    /// Gets the current file.
    /// </summary>
    /// <param name="solution">
    /// The solution.
    /// </param>
    /// <param name="dataContext">
    /// The data context.
    /// </param>
    /// <returns>
    /// The current file.
    /// </returns>
    private static FavoriteFilePath GetCurrentFile(ISolution solution, IDataContext dataContext)
    {
      var document = dataContext.GetData(DataConstants.DOCUMENT);
      if (document == null)
      {
        return null;
      }

      var projectFile = DocumentManager.GetInstance(solution).GetProjectFile(document);
      if (projectFile == null)
      {
        return null;
      }

      var result = new FavoriteFilePath();

      var path = projectFile.Location;

      var project = projectFile.GetProject();

      // miscellaneous files
      if (!project.IsWritable && project.LanguageType == ProjectFileType.UNKNOWN && project.Location.IsEmpty)
      {
        project = null;
      }

      if (project != null)
      {
        result.ProjectName = project.Name;
        path = projectFile.Location.ConvertToRelativePath(project.Location);
      }

      result.Path = path.ToString();

      return result;
    }

    /// <summary>
    /// Organizes this instance.
    /// </summary>
    private static void Organize()
    {
      using (var organizeDialog = new OrganizeDialog())
      {
        organizeDialog.ShowDialog();
      }
    }

    /// <summary>
    /// Adds the current file.
    /// </summary>
    private void AddCurrentFile()
    {
      if (this.currentFile == null)
      {
        return;
      }

      var favoriteFiles = FavoriteFilesSettings.Instance.FavoriteFiles;

      for (var n = favoriteFiles.Count - 1; n >= 0; n--)
      {
        var existingFavoriteFile = favoriteFiles[n];

        if (existingFavoriteFile.Path == this.currentFile.Path && existingFavoriteFile.ProjectName == this.currentFile.ProjectName)
        {
          favoriteFiles.RemoveAt(n);
        }
      }

      favoriteFiles.Add(this.currentFile);
    }

    /// <summary>
    /// Describes the add menu item.
    /// </summary>
    /// <returns>
    /// The add menu item.
    /// </returns>
    private SimpleMenuItem DescribeAddMenuItem()
    {
      var result = new SimpleMenuItem
      {
        Text = new global::JetBrains.UI.RichText.RichText("Add Current File")
      };

      result.Clicked += delegate { this.AddCurrentFile(); };

      if (this.currentFile != null)
      {
        result.Style = MenuItemStyle.Enabled;
      }

      return result;
    }

    /// <summary>
    /// Describes the more menu item.
    /// </summary>
    /// <returns>
    /// The more menu item.
    /// </returns>
    private SimpleMenuItem DescribeOrganizeMenuItem()
    {
      var result = new SimpleMenuItem
      {
        Text = new global::JetBrains.UI.RichText.RichText("Organize...")
      };

      result.Clicked += delegate { Organize(); };

      if (this.currentFile != null)
      {
        result.Style = MenuItemStyle.Enabled;
      }

      return result;
    }

    /// <summary>
    /// Executes action. Called after Update, that set <c>ActionPresentation</c>.Enabled to true.
    /// </summary>
    /// <param name="solution">
    /// The solution.
    /// </param>
    /// <param name="context">
    /// The context.
    /// </param>
    private void Execute(ISolution solution, IDataContext context)
    {
      this.solution = solution;
      this.currentFile = GetCurrentFile(this.solution, context);

      var items = new List<SimpleMenuItem>();

      var files = FavoriteFilesSettings.Instance.FavoriteFiles;

      var index = 0;

      foreach (var favoriteFilePath in files)
      {
        var path = favoriteFilePath;

        if (string.IsNullOrEmpty(favoriteFilePath.ProjectName))
        {
          var item = DescribeFavoriteFile(favoriteFilePath, index);

          item.Clicked += delegate { this.MenuItemClicked(path); };

          items.Add(item);

          index++;

          continue;
        }

        var project = solution.GetProject(favoriteFilePath.ProjectName);

        if (project != null)
        {
          var item = DescribeFavoriteFile(favoriteFilePath, index);

          item.Clicked += delegate { this.MenuItemClicked(path); };

          items.Add(item);

          index++;

          continue;
        }
      }

      if (items.Count > 0)
      {
        items.Add(SimpleMenuItem.CreateSeparator());
      }

      items.Add(this.DescribeAddMenuItem());
      items.Add(this.DescribeOrganizeMenuItem());

      var menu = new JetPopupMenu();

      menu.Caption.Value = WindowlessControl.Create("Favorite Files");
      menu.SetItems(items.ToArray());
      menu.KeyboardAcceleration.SetValue(KeyboardAccelerationFlags.Mnemonics);

      menu.Show();
    }

    /// <summary>
    /// Handles the ItemClicked event of the menu control.
    /// </summary>
    /// <param name="path">
    /// The path.
    /// </param>
    private void MenuItemClicked(FavoriteFilePath path)
    {
      if (path.Path == "__add")
      {
        this.AddCurrentFile();
      }
      else if (path.Path == "__more")
      {
        Organize();
      }
      else
      {
        this.OpenFavoriteFile(path);
      }
    }

    /// <summary>
    /// Opens the favorite file.
    /// </summary>
    /// <param name="favoriteFilePath">
    /// The favorite file path.
    /// </param>
    private void OpenFavoriteFile(FavoriteFilePath favoriteFilePath)
    {
      var path = new global::JetBrains.Util.FileSystemPath(favoriteFilePath.Path);

      if (string.IsNullOrEmpty(favoriteFilePath.ProjectName))
      {
        _DTE dte = IVsServiceProviderEx.Dte(VSShell.Instance.ServiceProvider);
        dte.ItemOperations.OpenFile(favoriteFilePath.Path, Constants.vsViewKindTextView);
        return;
      }

      if (!string.IsNullOrEmpty(favoriteFilePath.ProjectName))
      {
        var project = this.solution.GetProject(favoriteFilePath.ProjectName);

        if (project == null)
        {
          return;
        }

        var projectPath = project.Location;

        path = projectPath.Combine(path);
      }

      var location = this.solution.FindProjectItemsByLocation(path);
      if (location == null || global::JetBrains.Util.CollectionUtil.Length(location) == 0)
      {
        return;
      }

      var projectItem = location[0];

      if (projectItem == null)
      {
        System.Windows.Forms.MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      if (projectItem.Kind != ProjectItemKind.PHYSICAL_FILE)
      {
        System.Windows.Forms.MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      var projectFile = projectItem as IProjectFile;
      if (projectFile == null)
      {
        System.Windows.Forms.MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      if (EditorManager.GetInstance(this.solution).OpenProjectFile(projectFile, true) == null)
      {
        System.Windows.Forms.MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion
  }
}