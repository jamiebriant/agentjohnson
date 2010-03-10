// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizeDialog.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Organize dialog
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.FavoriteFiles
{
  using System;
  using System.Collections.Generic;
  using System.Windows.Forms;

  /// <summary>Represents the Organize dialog</summary>
  public partial class OrganizeDialog : Form
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizeDialog"/> class.
    /// </summary>
    public OrganizeDialog()
    {
      this.InitializeComponent();

      var files = new List<FavoriteFilePath>(FavoriteFilesSettings.Instance.FavoriteFiles);

      this.Listbox.BeginUpdate();

      foreach (var file in files)
      {
        this.Listbox.Items.Add(file);
      }

      this.Listbox.EndUpdate();

      this.EnableButtons();
    }

    #endregion

    #region Methods

    /// <summary>Handles the Delete button_ click event.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void DeleteButton_Click(object sender, EventArgs e)
    {
      var n = this.Listbox.SelectedIndex;

      if (n < 0)
      {
        return;
      }

      this.Listbox.BeginUpdate();

      this.Listbox.Items.RemoveAt(n);

      this.Listbox.EndUpdate();

      if (n >= this.Listbox.Items.Count)
      {
        n--;
      }

      this.Listbox.SelectedIndex = n;
    }

    /// <summary>Handles the Down button_ click event.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void DownButton_Click(object sender, EventArgs e)
    {
      var n = this.Listbox.SelectedIndex;

      if (n < 0 || n >= this.Listbox.Items.Count - 1)
      {
        return;
      }

      var selected = this.Listbox.Items[n];

      this.Listbox.BeginUpdate();

      this.Listbox.Items.RemoveAt(n);
      this.Listbox.Items.Insert(n + 1, selected);

      this.Listbox.EndUpdate();

      this.Listbox.SelectedIndex = n + 1;
    }

    /// <summary>Enables the buttons.</summary>
    private void EnableButtons()
    {
      var n = this.Listbox.SelectedIndex;

      this.DeleteButton.Enabled = n >= 0;
      this.UpButton.Enabled = n > 0;
      this.DownButton.Enabled = n >= 0 && n < this.Listbox.Items.Count - 1;
    }

    /// <summary>Handles the SelectedIndexChanged event of the Listbox control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void Listbox_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.EnableButtons();
    }

    /// <summary>Handles the OK button_ click event.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OKButton_Click(object sender, EventArgs e)
    {
      var files = new List<FavoriteFilePath>();

      foreach (FavoriteFilePath file in this.Listbox.Items)
      {
        files.Add(file);
      }

      FavoriteFilesSettings.Instance.FavoriteFiles = files;
    }

    /// <summary>Handles the Up button_ click event.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void UpButton_Click(object sender, EventArgs e)
    {
      var n = this.Listbox.SelectedIndex;

      if (n < 1)
      {
        return;
      }

      var selected = this.Listbox.Items[n];

      this.Listbox.BeginUpdate();

      this.Listbox.Items.RemoveAt(n);
      this.Listbox.Items.Insert(n - 1, selected);

      this.Listbox.EndUpdate();

      this.Listbox.SelectedIndex = n - 1;
    }

    #endregion
  }
}