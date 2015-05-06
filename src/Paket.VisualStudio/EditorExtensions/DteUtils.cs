using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace MadsKristensen.EditorExtensions
{
    public class DteUtils
    {
        private static DTE2 _dte;

        internal static DTE2 DTE
        {
            get { return _dte ?? (_dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2); }
        }

        public static IDisposable UndoContext(string name)
        {
            DTE.UndoContext.Open(name);

            return new Disposable(DTE.UndoContext.Close);
        }

    }
}