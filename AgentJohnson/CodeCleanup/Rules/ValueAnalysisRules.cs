// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueAnalysisRules.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the DocumentationRules type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.CodeCleanup.Rules
{
  #region Using Directives

  using System.Collections.Generic;
  using AgentJohnson.CodeCleanup.Options;
  using AgentJohnson.ValueAnalysis;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;

  #endregion

  /// <summary>Declaration comments fixes SA1600, SA1602, SA1611, SA1615, SA1617, SA1642.</summary>
  public class ValueAnalysisRules
  {
    #region Public Methods

    /// <summary>
    /// Executes the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="file">The file.</param>
    public void Execute(ValueAnalysisOptions options, ICSharpFile file)
    {
      foreach (var namespaceDeclaration in file.NamespaceDeclarations)
      {
        this.ProcessCSharpTypeDeclarations(options, namespaceDeclaration.TypeDeclarations);
      }

      this.ProcessCSharpTypeDeclarations(options, file.TypeDeclarations);
    }

    #endregion

    #region Methods

    /// <summary>Checks the declaration documentation.</summary>
    /// <param name="declaration">The declaration.</param>
    /// <param name="options">The options.</param>
    private void Annotate(IDeclaration declaration, ValueAnalysisOptions options)
    {
      if (!options.AnnotateWithValueAnalysisAttributes && !options.InsertAssertStatements)
      {
        return;
      }

      var typeMemberDeclaration = declaration as ITypeMemberDeclaration;
      if (typeMemberDeclaration == null)
      {
        return;
      }

      var refactoring = new ValueAnalysisRefactoring(typeMemberDeclaration);

      refactoring.AnnotateWithValueAnalysisAttributes = options.AnnotateWithValueAnalysisAttributes;
      refactoring.InsertAssertStatements = options.InsertAssertStatements;

      refactoring.Execute();
    }

    /// <summary>Processes the C sharp type declarations.</summary>
    /// <param name="options">The options.</param>
    /// <param name="typeDeclarations">The type declarations.</param>
    private void ProcessCSharpTypeDeclarations(ValueAnalysisOptions options, IEnumerable<ICSharpTypeDeclaration> typeDeclarations)
    {
      foreach (var typeDeclaration in typeDeclarations)
      {
        foreach (var memberDeclaration in typeDeclaration.MemberDeclarations)
        {
          this.Annotate(memberDeclaration, options);
        }

        this.ProcessNestedTypeDeclarations(options, typeDeclaration.NestedTypeDeclarations);
      }
    }

    /// <summary>Processes the nested type declarations.</summary>
    /// <param name="options">The options.</param>
    /// <param name="typeDeclarations">The type declarations.</param>
    private void ProcessNestedTypeDeclarations(ValueAnalysisOptions options, IEnumerable<ITypeDeclaration> typeDeclarations)
    {
      foreach (var typeDeclaration in typeDeclarations)
      {
        foreach (var memberDeclaration in typeDeclaration.MemberDeclarations)
        {
          this.Annotate(memberDeclaration, options);
        }

        this.ProcessNestedTypeDeclarations(options, typeDeclaration.NestedTypeDeclarations);
      }
    }

    #endregion
  }
}