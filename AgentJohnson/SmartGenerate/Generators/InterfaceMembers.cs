﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterfaceMembers.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The interface members.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.Generators
{
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>The interface members.</summary>
  [SmartGenerate("Generate interface members", "Generates a new property or method on an interface", Priority = 0)]
  public class InterfaceMembers : SmartGenerateHandlerBase
  {
    #region Methods

    /// <summary>Gets the items.</summary>
    /// <param name="smartGenerateParameters">The get menu items parameters.</param>
    protected override void GetItems(SmartGenerateParameters smartGenerateParameters)
    {
      var element = smartGenerateParameters.Element;

      var interfaceDeclaration = element.GetContainingElement(typeof(IInterfaceDeclaration), true);
      if (interfaceDeclaration == null)
      {
        return;
      }

      var memberDeclaration = element.GetContainingElement(typeof(ITypeMemberDeclaration), true);
      if (memberDeclaration != null && !(memberDeclaration is IInterfaceDeclaration))
      {
        return;
      }

      this.AddAction("Property", "D6EB42DA-2858-46B3-8CB3-E3DEFB245D11");
      this.AddAction("Method", "B3DB6158-D43E-42EE-8E67-F10CF7344106");
    }

    #endregion
  }
}