// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScopeEntry.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the scope entry class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.SmartGenerate.Scopes
{
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>Defines the scope entry class.</summary>
  public class ScopeEntry
  {
    #region Properties

    /// <summary>
    /// Gets or sets the element.
    /// </summary>
    /// <value>The element.</value>
    public IElement Element { get; set; }

    /// <summary>
    /// Gets or sets the expression.
    /// </summary>
    /// <value>The expression.</value>
    public IReferenceExpression Expression { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is assigned.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is assigned; otherwise, <c>false</c>.
    /// </value>
    public bool IsAssigned { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>The type.</value>
    public IType Type { get; set; }

    #endregion
  }
}