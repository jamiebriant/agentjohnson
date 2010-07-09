// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplementICloneableActionHandler.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the implement <c>ICloneable</c> action handler class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.Refactorings
{
  using System.Text;
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

  /// <summary>Defines the implement <c>ICloneable</c> action handler class.</summary>
  [UsedImplicitly]
  [ContextAction(Description = "Implement IClonable interface.", Name = "Implement IClonable [Agent Johnson]", Priority = -1, Group = "C#")]
  public class ImplementICloneable : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ImplementICloneable"/> class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public ImplementICloneable([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the specified context.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns><c>true</c>, if updateable.</returns>
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

        if (typeName == "System.ICloneable")
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
      return "Implement IClonable [Agent Johnson]";
    }

    /// <summary>Adds the dispose object method.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddCloneMethod(IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      var cls = classDeclaration.DeclaredElement as IClass;
      if (cls == null)
      {
        return;
      }

      var code = "var result = new " + cls.ShortName + "();";

      code += AddCloneMethodCode(cls, false);

      foreach (var declaredType in cls.GetSuperTypes())
      {
        var resolve = declaredType.Resolve();

        var superClass = resolve.DeclaredElement as IClass;
        if (superClass == null)
        {
          continue;
        }

        code += AddCloneMethodCode(superClass, true);
      }

      code += "\r\nreturn result;";

      AddMember(
        classDeclaration, 
        factory, 
        @"
        public object Clone() {" +
        code + @"
        }
      ");
    }

    /// <summary>Adds the get object data method code.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="isSuperClass">if set to <c>true</c> [is super class].</param>
    /// <returns>Returns the clone method code.</returns>
    private static string AddCloneMethodCode(IClass classDeclaration, bool isSuperClass)
    {
      var code = new StringBuilder();

      foreach (var field in classDeclaration.Fields)
      {
        if (isSuperClass)
        {
          var rights = field.GetAccessRights();
          if (rights == AccessRights.PRIVATE)
          {
            continue;
          }
        }

        code.Append(string.Format("\r\nresult.{0} = {0};", field.ShortName));
      }

      foreach (var property in classDeclaration.Properties)
      {
        if (isSuperClass)
        {
          var rights = property.GetAccessRights();
          if (rights == AccessRights.PRIVATE)
          {
            continue;
          }
        }

        var declarations = property.GetDeclarations();
        if (declarations.Count != 1)
        {
          continue;
        }

        var propertyDeclaration = declarations[0] as IPropertyDeclaration;
        if (propertyDeclaration == null || !propertyDeclaration.IsAuto)
        {
          continue;
        }

        code.Append(string.Format("\r\nresult.{0} = {0};", property.ShortName));
      }

      return code.ToString();
    }

    /// <summary>Adds the interface.</summary>
    /// <param name="solution">The solution.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    private static void AddInterface(ISolution solution, IClassDeclaration classDeclaration)
    {
      var scope = DeclarationsScopeFactory.SolutionScope(solution, true);
      var cache = PsiManager.GetInstance(solution).GetDeclarationsCache(scope, true);

      var typeElement = cache.GetTypeElementByCLRName("System.ICloneable");
      if (typeElement == null)
      {
        return;
      }

      var declaredType = TypeFactory.CreateType(typeElement);

      classDeclaration.AddSuperInterface(declaredType, false);
    }

    /// <summary>Adds the member.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    /// <param name="code">The member code.</param>
    private static void AddMember(IClassDeclaration classDeclaration, CSharpElementFactory factory, string code)
    {
      var memberDeclaration = factory.CreateTypeMemberDeclaration(code) as IClassMemberDeclaration;

      classDeclaration.AddClassMemberDeclaration(memberDeclaration);
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
      AddCloneMethod(classDeclaration, factory);
    }

    #endregion
  }
}