// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AgentJohnsonCodeCleanupModule.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the StyleCopForReSharperCodeCleanupModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.CodeCleanup
{
  #region Using Directives

  using System.Collections.Generic;
  using AgentJohnson.CodeCleanup.Descriptors;
  using AgentJohnson.CodeCleanup.Options;
  using AgentJohnson.CodeCleanup.Rules;
  using JetBrains.Application.Progress;
  using JetBrains.ProjectModel;
  using JetBrains.ReSharper.Feature.Services.CodeCleanup;
  using JetBrains.ReSharper.Feature.Services.CSharp.CodeCleanup;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Impl.PsiManagerImpl;

  #endregion

  /// <summary>Custom StyleCop for ReSharper CodeCleanUp module to fix StyleCop violations.
  /// We ensure that most of the ReSharper modules are run before we are so we can. </summary>
  [CodeCleanupModule(ModulesBefore = new[]
  {
    typeof(UpdateFileHeader), typeof(ArrangeThisQualifier), typeof(ReplaceByVar), typeof(ReformatCode)
  })]
  public class AgentJohnsonCodeCleanupModule : ICodeCleanupModule
  {
    #region Constants and Fields

    /// <summary>
    /// Documentation descriptor.
    /// </summary>
    private static readonly ValueAnalysisDescriptor valueAnalysisDescriptor = new ValueAnalysisDescriptor();

    #endregion

    #region Implemented Interfaces

    #region ICodeCleanupModule

    /// <summary>
    /// Gets the collection of option descriptors.
    /// </summary>
    /// <value>
    /// The descriptors.
    /// </value>
    public ICollection<CodeCleanupOptionDescriptor> Descriptors
    {
      get
      {
        return new CodeCleanupOptionDescriptor[]
        {
          valueAnalysisDescriptor
        };
      }
    }

    /// <summary>
    /// Gets a value indicating whether the module is available on selection, or on the whole file.
    /// </summary>
    /// <value>
    /// The is available on selection.
    /// </value>
    public bool IsAvailableOnSelection
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Gets the language this module can operate.
    /// </summary>
    /// <value>
    /// The language type.
    /// </value>
    public PsiLanguageType LanguageType
    {
      get
      {
        return CSharpLanguageService.CSHARP;
      }
    }

    #endregion

    #endregion

    #region Implemented Interfaces

    #region ICodeCleanupModule

    /// <summary>Check if this module can handle given project file.</summary>
    /// <param name="projectFile">The project file to check.</param>
    /// <returns><c>True.</c>if the project file is available; otherwise <c>False.</c>.</returns>
    public bool IsAvailable(IProjectFile projectFile)
    {
      var languageType = ProjectFileLanguageServiceManager.Instance.GetPrimaryPsiLanguageType(projectFile);
      return languageType == CSharpLanguageService.CSHARP;
    }

    /// <summary>Process clean-up on file.</summary>
    /// <param name="projectFile">The project file to process.</param>
    /// <param name="range">The range marker to process.</param>
    /// <param name="profile">The code cleanup settings to use.</param>
    /// <param name="canIncrementalUpdate">Determines whether we can incrementally update.</param>
    /// <param name="progressIndicator">The progress indicator.</param>
    public void Process(IProjectFile projectFile, IPsiRangeMarker range, CodeCleanupProfile profile, out bool canIncrementalUpdate, IProgressIndicator progressIndicator)
    {
      canIncrementalUpdate = true;

      var valueAnalysisOptions = profile.GetSetting(valueAnalysisDescriptor);
      if (!valueAnalysisOptions.AnnotateWithValueAnalysisAttribute)
      {
        return;
      }

      if (projectFile == null)
      {
        return;
      }

      if (!this.IsAvailable(projectFile))
      {
        return;
      }

      var solution = projectFile.GetSolution();

      var psiManager = PsiManagerImpl.GetInstance(solution);

      var file = psiManager.GetPsiFile(projectFile, PsiLanguageType.GetByProjectFile(projectFile)) as ICSharpFile;

      if (file == null)
      {
        return;
      }

      new ValueAnalysisRules().Execute(valueAnalysisOptions, file);
    }

    /// <summary>Get default setting for given profile type.</summary>
    /// <param name="profile">The code cleanup profile to use.</param>
    /// <param name="profileType">Determine if it is a full or reformat <see cref="CodeCleanup.DefaultProfileType"/>.</param>
    public void SetDefaultSetting(CodeCleanupProfile profile, CodeCleanup.DefaultProfileType profileType)
    {
      var documentationOptions = new ValueAnalysisOptions();
      profile.SetSetting(valueAnalysisDescriptor, documentationOptions);
    }

    #endregion

    #endregion
  }
}