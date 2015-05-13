using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

using VsShell = Microsoft.VisualStudio.Shell.VsShellUtilities;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Runtime.InteropServices;

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