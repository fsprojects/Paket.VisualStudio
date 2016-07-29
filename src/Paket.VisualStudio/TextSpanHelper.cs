using System;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Paket.VisualStudio
{
    [CLSCompliant(false)]
    public sealed class TextSpanHelper
    {
        private TextSpanHelper() { }

        public static bool IsPositive(TextSpan span)
        {
            return (span.iStartLine < span.iEndLine || (span.iStartLine == span.iEndLine && span.iStartIndex <= span.iEndIndex));
        }

        public static void MakePositive(ref TextSpan span)
        {
            if (!IsPositive(span))
            {
                int line;
                int idx;

                line = span.iStartLine;
                idx = span.iStartIndex;
                span.iStartLine = span.iEndLine;
                span.iStartIndex = span.iEndIndex;
                span.iEndLine = line;
                span.iEndIndex = idx;
            }

            return;
        }
    }
}