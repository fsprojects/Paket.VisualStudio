﻿using System;
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

        public GraphNode SelectedGraphNode { get; private set; }

        public ActiveGraphNodeTracker(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string GetSelectedFileName()
        {
            // Get the file path
            string itemFullPath = null;
            IVsHierarchy hierarchy = null;
            uint itemid = VSConstants.VSITEMID_NIL;
            if (!IsSingleProjectItemSelection(out hierarchy, out itemid))
                return null;

            ((IVsProject)hierarchy).GetMkDocument(itemid, out itemFullPath);
            return itemFullPath;
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
            if (pSCNew != null)
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

        public static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
        {
            hierarchy = null;
            itemid = VSConstants.VSITEMID_NIL;
            int hr = VSConstants.S_OK;

            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (monitorSelection == null || solution == null)
            {
                return false;
            }

            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            try
            {
                hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

                if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL)
                {
                    // there is no selection
                    return false;
                }

                // multiple items are selected
                if (multiItemSelect != null) return false;

                // there is a hierarchy root node selected, thus it is not a single item inside a project

                if (itemid == VSConstants.VSITEMID_ROOT) return false;

                hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                if (hierarchy == null) return false;

                Guid guidProjectID = Guid.Empty;

                if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID)))
                {
                    return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
                }

                // if we got this far then there is a single project item selected
                return true;
            }
            finally
            {
                if (selectionContainerPtr != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPtr);
                }

                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
            }
        }
    }
}