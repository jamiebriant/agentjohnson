using System.Collections.Generic;

namespace AgentJohnson.SmartGenerate {
  /// <summary>
  /// 
  /// </summary>
  public interface ILiveTemplate {
    #region Public methods

    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The items.</returns>
    IEnumerable<LiveTemplateItem> GetItems(SmartGenerateParameters parameters);

    #endregion
  }
}