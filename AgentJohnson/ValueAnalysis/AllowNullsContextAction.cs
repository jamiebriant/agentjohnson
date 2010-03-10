// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllowNullsContextAction.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents the Context Action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson.ValueAnalysis
{
  using AgentJohnson.Psi.CodeStyle;
  using JetBrains.ReSharper.Feature.Services.Bulbs;
  using JetBrains.ReSharper.Intentions.CSharp.DataProviders;
  using JetBrains.ReSharper.Psi;
  using JetBrains.ReSharper.Psi.Caches;
  using JetBrains.ReSharper.Psi.CSharp;
  using JetBrains.ReSharper.Psi.CSharp.Tree;
  using JetBrains.ReSharper.Psi.Tree;

  /// <summary>Represents the Context Action.</summary>
  [ContextAction(Description = "Annotates a function with the Allow Null attribute.", Name = "Annotate with Allow Null attributes for all parameters", Priority = -1, Group = "C#")]
  public class AllowNullsContextAction : ContextActionBase
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="AllowNullsContextAction"/> class.</summary>
    /// <param name="provider">The provider.</param>
    public AllowNullsContextAction(ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    #endregion

    #region Public Methods

    /// <summary>Called to check if ContextAction is available.
    /// ReadLock is taken
    /// Will not be called if PsiManager, ProjectFile of Solution == null</summary>
    /// <param name="element">The element.</param>
    /// <returns>The is available.</returns>
    public override bool IsAvailable(IElement element)
    {
      if (string.IsNullOrEmpty(ValueAnalysisSettings.Instance.AllowNullAttribute))
      {
        return false;
      }

      var typeMemberDeclaration = this.GetTypeMemberDeclaration();
      if (typeMemberDeclaration == null)
      {
        return false;
      }

      var typeElement = GetAttribute(typeMemberDeclaration, ValueAnalysisSettings.Instance.AllowNullAttribute);
      if (typeElement == null)
      {
        return false;
      }

      var parametersOwner = typeMemberDeclaration.DeclaredElement as IParametersOwner;
      if (parametersOwner == null || parametersOwner.Parameters.Count == 0)
      {
        return false;
      }

      IAttributesOwner attributesOwner = typeMemberDeclaration.DeclaredElement;
      if (attributesOwner == null)
      {
        return false;
      }

      var clrTypeName = new CLRTypeName(ValueAnalysisSettings.Instance.AllowNullAttribute);

      var attributeInstances = attributesOwner.GetAttributeInstances(clrTypeName, true);

      foreach (var instance in attributeInstances)
      {
        var allowNull = instance.PositionParameter(0).ConstantValue;

        if (allowNull.Value == null)
        {
          continue;
        }

        var allowNullName = allowNull.Value as string;

        if (allowNullName == "*")
        {
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Methods

    /// <summary>Executes the internal.</summary>
    /// <param name="element">The element.</param>
    protected override void Execute(IElement element)
    {
      var allowNullAttribute = ValueAnalysisSettings.Instance.AllowNullAttribute;
      if (string.IsNullOrEmpty(allowNullAttribute))
      {
        return;
      }

      if (!this.IsAvailableInternal())
      {
        return;
      }

      var typeMemberDeclaration = this.GetTypeMemberDeclaration();
      if (typeMemberDeclaration == null)
      {
        return;
      }

      var attributesOwnerDeclaration = typeMemberDeclaration as IAttributesOwnerDeclaration;
      if (attributesOwnerDeclaration == null)
      {
        return;
      }

      var typeElement = GetAttribute(typeMemberDeclaration, allowNullAttribute);
      if (typeElement == null)
      {
        return;
      }

      var factory = CSharpElementFactory.GetInstance(typeMemberDeclaration.GetPsiModule());

      var objects = new object[]
      {
      };

      var attribute = factory.CreateTypeMemberDeclaration("[" + allowNullAttribute + "(\"*\")]void Foo(){}", objects).Attributes[0];

      attribute = attributesOwnerDeclaration.AddAttributeAfter(attribute, null);

      var name = attribute.TypeReference.GetName();
      if (!name.EndsWith("Attribute"))
      {
        return;
      }

      /*
      IReferenceName referenceName = factory.CreateReferenceName(name.Substring(0, name.Length - "Attribute".Length), new object[0]);
      referenceName = attribute.Name.ReplaceBy(referenceName);

      if (referenceName.Reference.Resolve().DeclaredElement != typeElement)
      {
        referenceName.Reference.BindTo(typeElement);
      }
      */
      var range = attribute.GetDocumentRange();
      var codeFormatter = new CodeFormatter();
      codeFormatter.Format(this.Solution, range);
    }

    /// <summary>Gets the text.</summary>
    /// <value>The text.</value>
    /// <returns>The get text.</returns>
    protected override string GetText()
    {
      var attribute = ValueAnalysisSettings.Instance.AllowNullAttribute;

      var n = attribute.LastIndexOf('.');
      if (n >= 0)
      {
        attribute = attribute.Substring(n + 1);
      }

      return string.Format("Annotate with '{0}' [Agent Johnson]", attribute);
    }

    /// <summary>Gets the attribute.</summary>
    /// <param name="typeMemberDeclaration">The type member declaration.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <returns>Returns the attribute.</returns>
    private static ITypeElement GetAttribute(IElement typeMemberDeclaration, string attributeName)
    {
      var solution = typeMemberDeclaration.GetManager().Solution;

      var scope = DeclarationsScopeFactory.SolutionScope(solution, true);
      var cache = PsiManager.GetInstance(solution).GetDeclarationsCache(scope, true);

      var typeElement = cache.GetTypeElementByCLRName(attributeName);

      if (typeElement == null)
      {
        return null;
      }

      /*
      PredefinedType predefinedType = new PredefinedType(solution.SolutionProject);
      if(!TypeFactory.CreateType(typeElement).IsSubtypeOf(predefinedType.Attribute)) {
        return null;
      }
      */
      return typeElement;
    }

    /// <summary>Gets the type member declaration.</summary>
    /// <returns>The type member declaration.</returns>
    private ITypeMemberDeclaration GetTypeMemberDeclaration()
    {
      var element = this.Provider.SelectedElement;
      if (element == null)
      {
        return null;
      }

      ITypeMemberDeclaration typeMemberDeclaration = null;

      var treeNode = element as ITreeNode;
      if (treeNode != null)
      {
        typeMemberDeclaration = treeNode.Parent as ITypeMemberDeclaration;
      }

      if (typeMemberDeclaration == null)
      {
        var identifierNode = element as IIdentifierNode;

        if (identifierNode != null)
        {
          typeMemberDeclaration = identifierNode.Parent as ITypeMemberDeclaration;
        }
      }

      return typeMemberDeclaration;
    }

    #endregion
  }
}