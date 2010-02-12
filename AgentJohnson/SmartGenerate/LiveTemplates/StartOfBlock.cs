namespace AgentJohnson.SmartGenerate.LiveTemplates
{
  using System.Collections.Generic;

  /// <summary>
  /// The return.
  /// </summary>
  [LiveTemplate("At the start of a block", "Executes a Live Template at the start of a block.")]
  public class StartOfBlock : ILiveTemplate
  {
    #region Implemented Interfaces

    #region ILiveTemplate

    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    /// <param name="smartGenerateParameters">
    /// The smart generate parameters.
    /// </param>
    /// <returns>
    /// The items.
    /// </returns>
    public IEnumerable<LiveTemplateItem> GetItems(SmartGenerateParameters smartGenerateParameters)
    {
      var element = smartGenerateParameters.Element;

      if (!StatementUtil.IsBeforeFirstStatement(element))
      {
        return null;
      }

      var liveTemplateItem = new LiveTemplateItem
      {
        MenuText = "At the start of a block",
        Description = "At the start of a block",
        Shortcut = "At the start of a block"
      };

      return new List<LiveTemplateItem>
      {
        liveTemplateItem
      };
    }

    #endregion

    #endregion
  }
}