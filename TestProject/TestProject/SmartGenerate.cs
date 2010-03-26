// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmartGenerate.cs" company="">
//   
// </copyright>
// <summary>
//   Pipper enumeration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TestProject
{
  using System.Collections.Generic;
  using Sitecore;
  using Sitecore.Diagnostics;

  /// <summary>Pipper enumeration</summary>
  public enum Pipper
  {
    /// <summary>The "Huba" enum value.</summary>
    Huba, 

    /// <summary>The "Huba" enum value.</summary>
    Flips, 

    /// <summary>The "Huba" enum value.</summary>
    Nuller, 

    /// <summary>The "Huba" enum value.</summary>
    Value, 

    /// <summary>The "Huba" enum value.</summary>
    Pipper
  }

  /// <summary>Defines the o1 class.</summary>
  public class O1
  {
    #region Constants and Fields

    /// <summary>
    /// The j field.
    /// </summary>
    private int j;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the J.
    /// </summary>
    /// <value>The J value.</value>
    public int J
    {
      get
      {
        return this.j;
      }

      set
      {
        this.j = value;
      }
    }

    /// <summary>
    /// Gets or sets the O.
    /// </summary>
    /// <value>The O value.</value>
    public O1 O { get; set; }

    /// <summary>
    /// Gets or sets the v1.
    /// </summary>
    /// <value>The v1 value.</value>
    public int V1 { get; set; }

    /// <summary>
    /// Gets or sets the v2.
    /// </summary>
    /// <value>The v2 value.</value>
    public int V2 { get; set; }

    #endregion

    #region Public Methods

    /// <summary>Copies the method.</summary>
    public void CopyMethod()
    {
    }

    /// <summary>Gets the bool.</summary>
    /// <param name="test">The test variable.</param>
    /// <returns>The boolean.</returns>
    public virtual bool GetBool([AllowNull] string test)
    {
      Assert.ArgumentNotNull(test, "test");
      return test == "1";
    }

    #endregion
  }

  /// <summary>Defines the o2 class.</summary>
  public class O2 : O1
  {
    #region Public Methods

    /// <summary>Gets the bool.</summary>
    /// <param name="test">The test variable.</param>
    /// <returns>The boolean value.</returns>
    public override bool GetBool(string test)
    {
      return test == "2";
    }

    #endregion
  }

  /// <summary>Defines the smart generate class.</summary>
  public class SmartGenerate
  {
    #region Properties

    /// <summary>
    /// Gets or sets the test.
    /// </summary>
    /// <value>The test result.</value>
    [CanBeNull]
    public string Test
    {
      get
      {
        var array = "1,2,3".Split(',');

        var keys = new Dictionary<object, int>();

        // string s = string.Empty;
        // O1 o = new O1();
        // o.V1 = 1;
        this.GetBool();

        var list = new List<object>();

        foreach (var o in list)
        {

        }

        var j = 0;

        while (j < 0)
        {
          j++;
        }
           
        if (j > 0)
        {
        }

        for (var i = 0; i < 10; i++)
        {
        }

        var s = list.ToString();



        return null;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>Gets the bool.</summary>
    /// <returns>The bool result.</returns>
    public bool GetBool()
    {
      return false;
    }

    /// <summary>Methods this instance.</summary>
    public void Method()
    {
      var o = GetObject();

      if (o != null)
      {
      }

    }

    #endregion

    #region Methods

    /// <summary>Gets the object.</summary>
    /// <returns>The object.</returns>
    [CanBeNull]
    private static object GetObject()
    {
      return null;
    }

    #endregion
  }
}