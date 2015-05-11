using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal class ActiveGraphNodeTracker : IVsSelectionEvents
    {
        private readonly IServiceProvider serviceProvider;
        private IVsMonitorSelection vsMonitorSelection;
        private uint selectionEventsCookie;

        public IVsHierarchy SelectedHierarchy { get; private set; }
        public GraphNode SelectedGraphNode { get; private set; }

        public ActiveGraphNodeTracker(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Register()
        {
            if (vsMonitorSelection == null)
                vsMonitorSelection = GetMonitorSelection();

            if(vsMonitorSelection != null)
                vsMonitorSelection.AdviseSelectionEvents(this, out selectionEventsCookie);
        }

        public void Unregister()
        {
            if (vsMonitorSelection == null)
                vsMonitorSelection = GetMonitorSelection();

            if (vsMonitorSelection != null)
                vsMonitorSelection.UnadviseSelectionEvents(selectionEventsCookie);
        }

        private IVsMonitorSelection GetMonitorSelection()
        {
            return serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
        }

        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            SelectedGraphNode = GetCurrentSelectionGraphNode(pSCNew);
            return VSConstants.S_OK;
        }

        private GraphNode GetCurrentSelectionGraphNode(ISelectionContainer selectionContainer)
        {
            uint selectedObjectsCount;
            if (!ErrorHandler.Succeeded(selectionContainer.CountObjects(SelectionContainer.SELECTED, out selectedObjectsCount)) || selectedObjectsCount == 0) return null;

            object[] selectedObjects = new object[selectedObjectsCount];
            if (ErrorHandler.Succeeded(selectionContainer.GetObjects(SelectionContainer.SELECTED, selectedObjectsCount, selectedObjects)))
            {
                return (from obj in selectedObjects
                    select obj as ISelectedGraphNode
                    into selectedGraphNode
                    where selectedGraphNode != null
                    select selectedGraphNode.Node).FirstOrDefault();
            }

            return null;
        }
    }
}