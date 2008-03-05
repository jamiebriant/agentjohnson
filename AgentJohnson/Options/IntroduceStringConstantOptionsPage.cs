using System;
using System.Collections.Generic;
using System.Windows.Forms;
using JetBrains.UI.Options;

namespace AgentJohnson.Options {
  [OptionsPage("AgentJohnson.IntroduceStringConstant", "Introduce String Constant", null, ParentId = "CodeInspection")]
  public partial class IntroduceStringConstantOptionsPage : UserControl, IOptionsPage {
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroduceStringConstantOptionsPage"/> class.
    /// </summary>
    public IntroduceStringConstantOptionsPage() {
      InitializeComponent();
    }

    #endregion

    #region Public properties

    /// <summary>
    /// Control to be shown as page
    /// </summary>
    /// <value></value>
    public Control Control {
      get {
        return this;
      }
    }

    /// <summary>
    /// Gets the ID of this option page.
    /// <see cref="T:JetBrains.UI.Options.IOptionsDialog"/> or <see cref="T:JetBrains.UI.Options.OptionsPageDescriptor"/> could be used to retrieve the <see cref="T:JetBrains.UI.Options.OptionsManager"/> out of it.
    /// </summary>
    /// <value></value>
    public string Id {
      get {
        return "AgentJohnson.IntroduceStringConstant";
      }
    }

    #endregion

    ///<summary>
    ///Raises the <see cref="E:System.Windows.Forms.UserControl.Load"></see> event.
    ///</summary>
    ///
    ///<param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data. </param>
    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);

      IntroduceStringConstantSettings settings = IntroduceStringConstantSettings.Instance;

      List<string> classNames = settings.ClassNames;

      foreach(string className in classNames) {
        methodsGridView.Rows.Add(new string[] { className });
      }

      rbReplaceWithUnderscore.Checked = settings.ReplaceSpacesMode == 0;
      rbReplaceWithNothing.Checked = settings.ReplaceSpacesMode == 1;

      rbUppercase.Checked = settings.TransformIdentifierMode == 0;
      rbCamelCase.Checked = settings.TransformIdentifierMode == 1;
      rbLeave.Checked = settings.TransformIdentifierMode == 2;

      cbGenerateXmlComment.Checked = settings.GenerateXmlComment;
    }

    #region Public methods

    /// <summary>
    /// Check if the settings on the page are consistent, and page could be closed
    /// </summary>
    /// <returns><c>true</c> if page data is consistent</returns>
    public bool ValidatePage() {
      return true;
    }

    ///<summary>
    ///
    ///            Invoked when this page is selected/unselected in the tree.
    ///            
    ///</summary>
    ///
    ///<param name="activated"><c>True</c>, when page is selected; <c>False</c>, when page is unselected.</param>
    public void OnActivated(bool activated) {
    }

    /// <summary>
    /// Invoked when OK button in the options dialog is pressed
    /// If the page returns <c>false</c>, the the options dialog won't be closed, and focus
    /// will be put into this page
    /// </summary>
    /// <returns></returns>
    public bool OnOk() {
      IntroduceStringConstantSettings settings = IntroduceStringConstantSettings.Instance;

      List<string> classNames = settings.ClassNames;
      classNames.Clear();

      DataGridViewRowCollection rows = methodsGridView.Rows;

      foreach(DataGridViewRow row in rows){
        object value = row.Cells[0].Value;

        if(value != null){
          classNames.Add(value.ToString());
        }
      }

      if (rbReplaceWithUnderscore.Checked){
        settings.ReplaceSpacesMode = 0;
      }
      else{
        settings.ReplaceSpacesMode = 1;
      }

      if (rbUppercase.Checked){
        settings.TransformIdentifierMode = 0;
      }
      else if (rbCamelCase.Checked){
        settings.TransformIdentifierMode = 1;
      }
      else{
        settings.TransformIdentifierMode = 2;
      }

      settings.GenerateXmlComment = cbGenerateXmlComment.Checked;

      return true;
    }

    #endregion
  }
}