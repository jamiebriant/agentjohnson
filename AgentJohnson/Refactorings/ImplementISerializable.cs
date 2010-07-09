// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplementISerializableActionHandler.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Defines the implement <c>ISerializable</c> action handler class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.Refactorings
{
  using JetBrains.Annotations;
  using JetBrains.Application;
  using JetBrains.DocumentModel;
  using JetBrains.ProjectModel;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.Caches;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;
  using JetBrains.ReSharper.Psi.Util;
  using JetBrains.Util;

  /// <summary>Defines the implement <c>ISerializable</c> action handler class.</summary>
  [UsedImplicitly]
  [ContextAction(Description = "Implement ISerializable interface.", Name = "Implement ISerializable [Agent Johnson]", Priority = -1, Group = "C#")]
  public class ImplementISerializable : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ImplementISerializable"/> class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public ImplementISerializable([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the specified context.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns><c>true</c>, if update is available.</returns>
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

        if (typeName == "System.Runtime.Serialization.ISerializable")
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

      using (var cookie = DocumentManager.GetInstance(this.Solution).EnsureWritable())
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
      return "Implement ISerializable [Agent Johnson]";
    }

    /// <summary>Adds the attribute.</summary>
    /// <param name="solution">The solution.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddAttribute(ISolution solution, IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      var typeName = new CLRTypeName("System.SerializableAttribute");

      var scope = DeclarationsScopeFactory.SolutionScope(solution, true);
      var cache = PsiManager.GetInstance(solution).GetDeclarationsCache(scope, true);

      var typeElement = cache.GetTypeElementByCLRName(typeName);
      if (typeElement == null)
      {
        return;
      }

      var attribute = factory.CreateAttribute(typeElement);

      classDeclaration.AddAttributeAfter(attribute, null);
    }

    /// <summary>Adds the dispose object method.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddConstructor(IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      var cls = classDeclaration.DeclaredElement as IClass;
      if (cls == null)
      {
        return;
      }

      var code = AddConstructorCode(cls, false);

      foreach (var declaredType in cls.GetSuperTypes())
      {
        var resolve = declaredType.Resolve();

        var superClass = resolve.DeclaredElement as IClass;
        if (superClass == null)
        {
          continue;
        }

        code += AddConstructorCode(superClass, true);
      }

      AddMember(
        classDeclaration,
        factory,
        @"protected Constructor(SerializationInfo info, StreamingContext context) {" + code + @"
        }
      ");
    }

    /// <summary>Adds the constructor code.</summary>
    /// <param name="cls">The class declaration.</param>
    /// <param name="isSuperClass">if set to <c>true</c> [is super class].</param>
    /// <returns>Returns the constructor code.</returns>
    private static string AddConstructorCode(IClass cls, bool isSuperClass)
    {
      var code = string.Empty;

      foreach (var field in cls.Fields)
      {
        if (isSuperClass)
        {
          var rights = field.GetAccessRights();
          if (rights == AccessRights.PRIVATE)
          {
            continue;
          }
        }

        var name = field.ShortName;
        var type = field.Type.GetPresentableName(cls.Language);

        if (field.Type.IsReferenceType())
        {
          code += string.Format("\r\n{0} = info.GetValue(\"{0}\", typeof({1})) as {1};", name, type);
        }
        else
        {
          code += string.Format("\r\n{0} = ({1})info.GetValue(\"{0}\", typeof({1}));", name, type);
        }
      }

      foreach (var property in cls.Properties)
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

        var name = property.ShortName;
        var type = property.Type.GetPresentableName(cls.Language);

        if (property.Type.IsReferenceType())
        {
          code += string.Format("\r\n{0} = info.GetValue(\"{0}\", typeof({1})) as {1};", name, type);
        }
        else
        {
          code += string.Format("\r\n{0} = ({1})info.GetValue(\"{0}\", typeof({1}));", name, type);
        }
      }

      return code;
    }

    /// <summary>Adds the dispose object method.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="factory">The factory.</param>
    private static void AddGetObjectDataMethod(IClassDeclaration classDeclaration, CSharpElementFactory factory)
    {
      var cls = classDeclaration.DeclaredElement as IClass;
      if (cls == null)
      {
        return;
      }

      var code = AddGetObjectDataMethodCode(cls, false);

      foreach (var declaredType in cls.GetSuperTypes())
      {
        var resolve = declaredType.Resolve();

        var superClass = resolve.DeclaredElement as IClass;
        if (superClass == null)
        {
          continue;
        }

        code += AddGetObjectDataMethodCode(superClass, true);
      }

      AddMember(
        classDeclaration,
        factory,
        @"
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {" + code + @"
        }
      ");
    }

    /// <summary>Adds the get object data method code.</summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="isSuperClass">if set to <c>true</c> [is super class].</param>
    /// <returns>Returns the get object data method code.</returns>
    private static string AddGetObjectDataMethodCode(IClass classDeclaration, bool isSuperClass)
    {
      var code = string.Empty;

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

        code += string.Format("\r\ninfo.AddValue(\"{0}\", {0}, {0}.GetType());", field.ShortName);
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

        code += string.Format("\r\ninfo.AddValue(\"{0}\", {0}, {0}.GetType());", property.ShortName);
      }

      return code;
    }

    /// <summary>Adds the interface.</summary>
    /// <param name="solution">The solution.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    private static void AddInterface(ISolution solution, IClassDeclaration classDeclaration)
    {
      var scope = DeclarationsScopeFactory.SolutionScope(solution, true);
      var cache = PsiManager.GetInstance(solution).GetDeclarationsCache(scope, true);

      var typeElement = cache.GetTypeElementByCLRName("System.Runtime.Serialization.ISerializable");
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
      AddAttribute(solution, classDeclaration, factory);
      AddConstructor(classDeclaration, factory);
      AddGetObjectDataMethod(classDeclaration, factory);
    }

    #endregion
  }
}