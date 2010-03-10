// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueAnalysisOptions.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the DocumentationOptions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.CodeCleanup.Options
{
  #region Using Directives

  using System.ComponentModel;
  using System.Reflection;
  using System.Text;

  #endregion

  /// <summary>Defines options for SCfR#.</summary>
  public class ValueAnalysisOptions
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueAnalysisOptions"/> class.
    /// </summary>
    public ValueAnalysisOptions()
    {
      this.AnnotateWithValueAnalysisAttribute = true;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the elements must be documented.
    /// </summary>
    [DisplayName("Annotate with Value Analysis attributes")]
    public bool AnnotateWithValueAnalysisAttribute { get; set; }

    #endregion

    #region Public Methods

    /// <summary>Returns a concatenated summary of the current options settings.</summary>
    /// <returns>A String of the options.</returns>
    public override string ToString()
    {
      var sb = new StringBuilder();
      var properties = this.GetType().GetProperties();

      for (var i = 0; i < properties.Length; i++)
      {
        var property = properties[i];
        if (i > 0)
        {
          sb.Append(", ");
        }

        sb.Append(this.GetPropertyDecription(property));
      }

      return sb.ToString();
    }

    #endregion

    #region Methods

    /// <summary>Builds a string reperesentation of the property value.</summary>
    /// <param name="propertyInfo">The propertyInfo to build the description for.</param>
    /// <returns>The string representation.</returns>
    private string GetPropertyDecription(PropertyInfo propertyInfo)
    {
      var propertyValue = propertyInfo.GetValue(this, null).ToString();

      var propName = string.Empty;
      var propValue = string.Empty;
      var displayNameAttributes = (DisplayNameAttribute[])propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false);
      if (displayNameAttributes.Length == 1)
      {
        propName = displayNameAttributes[0].DisplayName;
      }

      if (propertyInfo.PropertyType == typeof(bool))
      {
        propValue = propertyValue == "True" ? "Yes" : "No";
      }
      else
      {
        var field = propertyInfo.PropertyType.GetField(propertyValue);

        if (field != null)
        {
          var descriptionAttributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
          if (descriptionAttributes.Length == 1)
          {
            propValue = descriptionAttributes[0].Description;
          }
        }
      }

      return string.Format("{0} = {1}", propName, propValue);
    }

    #endregion
  }
}