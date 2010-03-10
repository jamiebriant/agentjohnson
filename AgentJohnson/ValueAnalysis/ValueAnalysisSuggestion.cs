// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueAnalysisSuggestion.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The value analysis suggestion.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.ValueAnalysis
{
  using JetBrains.ReSharper.Daemon;
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>The value analysis suggestion.</summary>
  [ConfigurableSeverityHighlighting(Name)]
  public class ValueAnalysisSuggestion : SuggestionBase
  {
    #region Constants and Fields

    /// <summary>
    /// The name.
    /// </summary>
    public const string Name = "ValueAnalysis";

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueAnalysisSuggestion"/> class.
    /// </summary>
    /// <param name="typeMemberDeclaration">The type member declaration.</param>
    public ValueAnalysisSuggestion(ITypeMemberDeclaration typeMemberDeclaration)
      : base(Name, typeMemberDeclaration, typeMemberDeclaration.GetNameDocumentRange(), "Type members should be annotated with Value Analysis attributes. [Agent Johnson]")
    {
    }

    #endregion

    #region Properties

    /// <summary>
    /// Get the severity of this highlighting
    /// </summary>
    /// <value></value>
    public override Severity Severity
    {
      get
      {
        var severity = HighlightingSettingsManager.Instance.Settings.GetSeverity(Name);
        return severity == Severity.DO_NOT_SHOW ? severity : Severity.WARNING;
      }
    }

    #endregion
  }
}