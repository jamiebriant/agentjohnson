﻿using JetBrains.ActionManagement;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace AgentJohnson.SmartGenerate {
  /// <summary>
  /// </summary>
  [SmartGenerate("Generate struct members", "Generates a property or method in a struct.", Priority=0)]
  public class GenerateStructMembers : SmartGenerateBase {
    #region Public methods

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="solution">The solution.</param>
    /// <param name="context">The context.</param>
    /// <param name="element">The element.</param>
    /// <returns>The items.</returns>
    protected override void GetItems(ISolution solution, IDataContext context, IElement element) {
      IStructDeclaration structDeclaration = element.GetContainingElement(typeof(IStructDeclaration), true) as IStructDeclaration;
      if(structDeclaration == null) {
        return;
      }

      IElement memberDeclaration = element.GetContainingElement(typeof(IClassMemberDeclaration), true);
      if(memberDeclaration != null && !(memberDeclaration is IStructDeclaration)) {
        return;
      }

      string modifier = GetModifier(element, structDeclaration);

      AddMenuItem("Auto property", "166BE49C-D068-476D-BC9C-2B5C3AF21B06", modifier);
      AddMenuItem("Property", "a684b217-f179-431b-a485-e3d76dbe57fd", modifier);
      AddMenuItem("Method", "85BBC654-4EE4-4932-BB0C-E0670FA1BB82", modifier);
    }

    #endregion
  }
}