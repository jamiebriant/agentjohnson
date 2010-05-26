// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterStatement.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   The parameter statement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.ValueAnalysis
{
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp.Tree;

  /// <summary>The parameter statement.</summary>
  internal class ParameterStatement
  {
    #region Constants and Fields

    /// <summary>
    /// The _attribute instance.
    /// </summary>
    private IAttributeInstance attributeInstance;

    /// <summary>
    /// The _needs statement.
    /// </summary>
    private bool needsStatement = true;

    /// <summary>
    /// The _nullable.
    /// </summary>
    private bool nullable;

    /// <summary>
    /// The _parameter.
    /// </summary>
    private IParameter parameter;

    /// <summary>
    /// The _statement.
    /// </summary>
    private IStatement statement;

    private string code;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the attribute.
    /// </summary>
    /// <value>The attribute.</value>
    public IAttributeInstance AttributeInstance
    {
      get
      {
        return this.attributeInstance;
      }

      set
      {
        this.attributeInstance = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [needs statement].
    /// </summary>
    /// <value><c>true</c> if [needs statement]; otherwise, <c>false</c>.</value>
    public bool NeedsStatement
    {
      get
      {
        return this.needsStatement;
      }

      set
      {
        this.needsStatement = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ParameterStatement"/> is nullable.
    /// </summary>
    /// <value><c>true</c> if nullable; otherwise, <c>false</c>.</value>
    public bool Nullable
    {
      get
      {
        return this.nullable;
      }

      set
      {
        this.nullable = value;
      }
    }

    /// <summary>
    /// Gets or sets the parameter.
    /// </summary>
    /// <value>The parameter.</value>
    public IParameter Parameter
    {
      get
      {
        return this.parameter;
      }

      set
      {
        this.parameter = value;
      }
    }

    /// <summary>
    /// Gets or sets the statement.
    /// </summary>
    /// <value>The statement.</value>
    public IStatement Statement
    {
      get
      {
        return this.statement;
      }

      set
      {
        this.statement = value;
      }
    }

    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    /// <value>The code.</value>
    public string Code
    {
      get
      {
        return code ?? string.Empty;
      }

      set
      {
        code = value;
      }
    }

    #endregion
  }
}