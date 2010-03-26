// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NegateIfStatement.cs" company="">
//   
// </copyright>
// <summary>
//   The negate if statement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TestProject
{
  /// <summary>The negate if statement.</summary>
  public class NegateIfStatement
  {
    #region Public Methods

    /// <summary>Tests this instance.</summary>
    public void Test()
    {
      var v = GetInt();

      if (v + 2 < v)
      {
        return;
      }

      if (v == 2)
      {
        return;
      }

      if (GetBool())
      {
        return;
      }

      if (true)
      {
        return;
      }
    }

    #endregion

    #region Methods

    /// <summary>Gets the boolean.</summary>
    /// <returns>The get bool.</returns>
    private static bool GetBool()
    {
      return false;
    }

    /// <summary>Gets the integer.</summary>
    /// <returns>The get int.</returns>
    private static int GetInt()
    {
      return 0;
    }

    #endregion
  }
}