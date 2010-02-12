namespace AgentJohnson.SmartGenerate.LiveTemplates
{
  using System.Collections.Generic;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>
  /// The return.
  /// </summary>
  [LiveTemplate("At the start of a method", "Executes a Live Template at the start of a method.")]
  public class MethodBegin : ILiveTemplate
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


      var block = element.GetContainingElement(typeof(IBlock), true) as IBlock;
      if (block == null)
      {
        return null;
      }

      var methodDeclaration = element.GetContainingElement(typeof(IMethodDeclaration), true) as IMethodDeclaration;
      if (methodDeclaration == null)
      {
        return null;
      }

      if (block != methodDeclaration.Body)
      {
        return null;
      }                                 

      var statement = block.Statements[0];
      var statementRange = statement.GetDocumentRange();
      var statementStart = statementRange.TextRange.StartOffset;

      if (element.GetTreeTextRange().StartOffset.Offset > statementStart)
      {
        return null;
      }

      var liveTemplateItem = new LiveTemplateItem
      {
        MenuText = "At the start of a method",
        Description = "At the start of a method",
        Shortcut = "At the start of a method"
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