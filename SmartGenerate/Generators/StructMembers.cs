﻿namespace AgentJohnson.SmartGenerate.Generators
{
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>
  /// </summary>
  [SmartGenerate("Generate struct members", "Generates a property or method in a struct.", Priority = 0)]
  public class StructMembers : SmartGenerateHandlerBase
  {
    #region Protected methods

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="smartGenerateParameters">The get menu items parameters.</param>
    protected override void GetItems(SmartGenerateParameters smartGenerateParameters)
    {
      IElement element = smartGenerateParameters.Element;

      IStructDeclaration structDeclaration = element.GetContainingElement(typeof(IStructDeclaration), true) as IStructDeclaration;
      if (structDeclaration == null)
      {
        return;
      }

      IElement memberDeclaration = element.GetContainingElement(typeof(IClassMemberDeclaration), true);
      if (memberDeclaration != null && !(memberDeclaration is IStructDeclaration))
      {
        return;
      }

      string modifier = ModifierUtil.GetModifier(element, structDeclaration);

      this.AddAction("Auto property", "166BE49C-D068-476D-BC9C-2B5C3AF21B06", modifier);
      this.AddAction("Property", "a684b217-f179-431b-a485-e3d76dbe57fd", modifier);
      this.AddAction("Method", "85BBC654-4EE4-4932-BB0C-E0670FA1BB82", modifier);
    }

    #endregion
  }
}