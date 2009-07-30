using JetBrains.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.Annotations;

namespace AgentJohnson.Util
{
    public static class Conversion
    {
        public static TextRange TreeTextRangeToTextRange([NotNull] TreeTextRange ttr)
        {
            return new TextRange(ttr.StartOffset.Offset, ttr.EndOffset.Offset);
        }
    }
}
