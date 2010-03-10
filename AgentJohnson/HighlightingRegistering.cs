// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighlightingRegistering.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Registers Agent Smith highlighters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson
{
  using AgentJohnson.Exceptions;
  using AgentJohnson.Strings;
  using AgentJohnson.ValueAnalysis;
  using JetBrains.Application;
  using JetBrains.ComponentModel;
  using JetBrains.ReSharper.Daemon;

  /// <summary>Registers Agent Smith highlighters.</summary>
  [ShellComponentImplementation(ProgramConfigurations.ALL)]
  public class HighlightingRegistering : IShellComponent
  {
    #region Implemented Interfaces

    #region IComponent

    /// <summary>Inits this instance.</summary>
    public void Init()
    {
      const string Group = "Agent Johnson";

      var manager = HighlightingSettingsManager.Instance;

      manager.RegisterConfigurableSeverity(
        DocumentThrownExceptionWarning.Name, 
        Group, 
        "Undocumented thrown exception.", 
        "Thrown exceptions should be documented in XML comments.", 
        Severity.WARNING);

      manager.RegisterConfigurableSeverity(
        StringEmptySuggestion.NAME, 
        Group, 
        "Replace \"\" with string.Empty.", 
        "Empty string literals (\"\") should be replaced with string.Empty.", 
        Severity.SUGGESTION);

      manager.RegisterConfigurableSeverity(
        ValueAnalysisSuggestion.Name, 
        Group, 
        "Annotate type members with Value Analysis attributes and assert statements.", 
        "Type members should be annotated with Value Analysis attributes and have assert statements.", 
        Severity.WARNING);

      manager.RegisterConfigurableSeverity(
        ReturnWarning.Name, 
        Group, 
        "Return values should be asserted.", 
        "The return value must not null and should be asserted.", 
        Severity.WARNING);
    }

    #endregion

    #region IDisposable

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
    }

    #endregion

    #endregion
  }
}