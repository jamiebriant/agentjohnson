namespace TestProject
{
  using System.IO;

  /// <summary>The catch exceptions.</summary>
  public class CatchExceptions
  {
    #region Public Methods

    /// <summary>Tests this instance.</summary>
    public void Test()
    {
      File.Delete("e:\\1.txt");
    }

    #endregion
  }
}