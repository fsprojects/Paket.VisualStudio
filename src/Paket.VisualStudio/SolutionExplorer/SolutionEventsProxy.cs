using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Paket.VisualStudio.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal class SolutionEventsProxy
    {
        internal SolutionEventsProxy(IServiceProvider serviceProvider)
        {
            solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;

            solutionEventsProxy = new _SolutionEventsProxy();
            solution.AdviseSolutionEvents(solutionEventsProxy, out handle);

            solutionEventsProxy.ProjectOpened.Subscribe(isOpened =>
            {
                if (openedDocuments == null || !openedDocuments.Any()) return;

                SolutionExplorerExtensions.OpenFiles(this.openedDocuments)
                                          .ToList()
                                          .ForEach(file => this.openedDocuments.Remove(file));
            });
        }

        private uint handle;
        private IVsSolution solution;
        private List<string> openedDocuments;

        private _SolutionEventsProxy solutionEventsProxy { get; }

        /// <summary>
        /// Track opened documents in projects, if they are in solution folders
        /// they will not be unloaded, so we can ignore them
        /// </summary>
        internal void TrackOpenedDocuments()
        {
            openedDocuments = DteHelper.DTE.Documents
                                       .Cast<EnvDTE.Document>()
                                       .Where(d => d.ProjectItem?.ContainingProject?.Kind != ProjectKinds.vsProjectKindSolutionFolder)
                                       .Select(d => d.FullName).ToList();
        }

        class _SolutionEventsProxy : IVsSolutionEvents3
        {
            public ISubject<bool> ProjectOpened { get; private set; }

            public _SolutionEventsProxy()
            {
                this.ProjectOpened = new Subject<bool>();
            }

            public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
            {
                this.ProjectOpened.OnNext(true);
                return VSConstants.S_OK;
            }
            public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) { return VSConstants.S_OK; }
            public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) { return VSConstants.S_OK; }
            public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) { return VSConstants.S_OK; }
            public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) { return VSConstants.S_OK; }
            public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) { return VSConstants.S_OK; }
            public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) { return VSConstants.S_OK; }
            public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) { return VSConstants.S_OK; }
            public int OnBeforeCloseSolution(object pUnkReserved) { return VSConstants.S_OK; }
            public int OnAfterCloseSolution(object pUnkReserved) { return VSConstants.S_OK; }
            public int OnAfterMergeSolution(object pUnkReserved) { return VSConstants.S_OK; }
            public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
            public int OnAfterOpeningChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
            public int OnBeforeClosingChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
            public int OnAfterClosingChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
        }
    }
}
