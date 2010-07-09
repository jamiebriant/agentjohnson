// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplementIDisposableActionHandler.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the implement I disposable action handler class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.Refactorings
{
  using JetBrains.Annotations;
  using JetBrains.Application;
  using JetBrains.ProjectModel;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.Caches;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.Util;

  /// <summary>Defines the implement I disposable action handler class.</summary>
  [UsedImplicitly]
  [ContextAction(Description = "Implement IDisposable interface.", Name = "Implement IDisposable [Agent Johnson]", Priority = -1, Group = "C#")]
  public class ImplementIDisposable : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ImplementIDisposable"/> class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public ImplementIDisposable([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the specified context.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns><c>true</c>, if the update succeeds.</returns>
    public override bool IsAvailable(IElement element)
    {
      var classDeclaration = element.ToTreeNode().Parent as IClassDeclaration;
      if (classDeclaration == null)
      {
        return false;
      }

      var types = classDeclaration.DeclaredElement.GetSuperTypes();

      foreach (var type in types)
      {
        var typeName = type.GetLongPresentableName(element.Language);

        if (typeName == "System.IDisposable")
        {
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes action. Called after Update, that set <c>ActionPresentation.Enabled</c> to true.
    /// </summary>
    /// <param name="element">The element.</param>
    protected override void Execute(IElement element)
    {
      var classDeclaration = element.ToTreeNode().Parent as IClassDeclaration;
      if (classDeclaration == null)
      {
        return;
      }

      using (var cookie = this.EnsureWritable())
      {
        if (cookie.EnsureWritableResult != EnsureWritableResult.SUCCESS)
        {
          return;
        }

        Execute(this.Solution, classDeclaration);
      }
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    /// <returns>The context action text.</returns>
    protected override string GetText()
    {
      return "Implement IDisposable [Agent Johnson]";
    }

    /// <summary>Adds the destructor.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddDestructor(IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      const string code = @"~Disposable() {
          DisposeObject(false);
        }";

      var memberDeclaration = factory.CreateTypeMemberDeclaration(code) as IClassMemberDeclaration;

      classDeclaration.AddClassMemberDeclaration(memberDeclaration);
    }

    /// <summary>Adds the dispose method.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddDisposeMethod(IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      const string code = @"
        public void Dispose() {
          DisposeObject(true);
          GC.SuppressFinalize(this);
        }";

      var memberDeclaration = factory.CreateTypeMemberDeclaration(code) as IClassMemberDeclaration;

      classDeclaration.AddClassMemberDeclaration(memberDeclaration);
    }

    /// <summary>Adds the dispose object method.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddDisposeObjectMethod(IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      const string code = @"
        void DisposeObject(bool disposing) {
          if(_disposed) {
            return;
          }
          if (disposing) {
            // Dispose managed resources.
          }
          // Dispose unmanaged resources.
          _disposed = true;         
        }";

      var memberDeclaration = factory.CreateTypeMemberDeclaration(code) as IClassMemberDeclaration;

      classDeclaration.AddClassMemberDeclaration(memberDeclaration);
    }

    /// <summary>Adds the dispose object method.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddField(IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      const string code = @"
        bool _disposed;
        ";

      var memberDeclaration = factory.CreateTypeMemberDeclaration(code) as IClassMemberDeclaration;

      classDeclaration.AddClassMemberDeclaration(memberDeclaration);
    }

    /// <summary>Adds the interface.</summary>
    /// <param name="solution">The solution.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    private static void AddInterface(ISolution solution, IClassDeclaration classDeclaration)
    {
      var scope = DeclarationsScopeFactory.SolutionScope(solution, true);
      var cache = PsiManager.GetInstance(solution).GetDeclarationsCache(scope, true);

      var typeElement = cache.GetTypeElementByCLRName("System.IDisposable");
      if (typeElement == null)
      {
        return;
      }

      var declaredType = TypeFactory.CreateType(typeElement);

      classDeclaration.AddSuperInterface(declaredType, false);
    }

    /// <summary>Executes the specified class declaration.</summary>
    /// <param name="solution">The solution.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    private static void Execute(ISolution solution, IClassDeclaration classDeclaration)
    {
      var factory = CSharpElementFactory.GetInstance(classDeclaration.GetPsiModule());
      if (factory == null)
      {
        return;
      }

      AddInterface(solution, classDeclaration);
      AddField(classDeclaration, factory);
      AddDestructor(classDeclaration, factory);
      AddDisposeMethod(classDeclaration, factory);
      AddDisposeObjectMethod(classDeclaration, factory);
    }

    #endregion
  }
}