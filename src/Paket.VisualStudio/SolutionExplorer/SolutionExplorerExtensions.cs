using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using MadsKristensen.EditorExtensions;
using EnvDTE;
using EnvDTE80;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal class SolutionExplorerExtensions
    {
        private static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static string GetSolutionDirectory()
        {
            string dir = null;
            string fileName = null;
            string userOptsFile = null;
            IVsSolution solution = _serviceProvider.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsSolution)) as IVsSolution;            

            solution.GetSolutionInfo(out dir, out fileName, out userOptsFile);
            return dir;            
        }

        public static string GetSolutionFileName()
        {
            string dir = null;
            string fileName = null;
            string userOptsFile = null;
            IVsSolution solution = _serviceProvider.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsSolution)) as IVsSolution;

            solution.GetSolutionInfo(out dir, out fileName, out userOptsFile);
            return fileName;
        }

        public static void SaveSolution()
        {
            if (DteUtils.DTE.Solution.IsDirty)
            {
                StatusBarService.UpdateText("Saving the current solution...");

                IVsSolution solution = _serviceProvider.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsSolution)) as IVsSolution;
                solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave, null, 0);
            }
        }

        public static void UnloadProject(Guid projectGuid)
        {
            if (projectGuid == Guid.Empty)
                return;

            IVsSolution4 solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution4;
            int hr;

            hr = solution.UnloadProject(ref projectGuid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);
            ErrorHandler.ThrowOnFailure(hr);
        }

        public static void ReloadProject(Guid projectGuid)
        {
            if (projectGuid == Guid.Empty)
                return;

            IVsSolution4 solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution4;
            int hr;

            hr = solution.ReloadProject(ref projectGuid);
            ErrorHandler.ThrowOnFailure(hr);
        }

        public static List<Guid> GetAllProjectGuids()
        {
            var projectGuids = new List<Guid>();

            IVsHierarchy hierarchy;
            IVsSolution solution = _serviceProvider.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsSolution)) as IVsSolution;            

            foreach (Project project in DteUtils.DTE.Solution.Projects)
            {
                solution.GetProjectOfUniqueName(project.FullName, out hierarchy);

                if (hierarchy != null)
                {
                    Guid projectGuid = Guid.Empty;
                    solution.GetGuidOfProject(hierarchy, out projectGuid);
                    projectGuids.Add(projectGuid);
                }
            }
            return projectGuids;
        }

        public static IEnumerable<Project> GetAllProjects()
        {
            return DteUtils.DTE.Solution.Projects
                .OfType<Project>()
                .SelectMany(GetProjects)
                .Where(p => File.Exists(p.FullName));
        }

        private static IEnumerable<Project> GetProjects(Project project)
        {
            if (project == null)
                return Enumerable.Empty<Project>();

            if (project.Kind != ProjectKinds.vsProjectKindSolutionFolder)
                return new[] { project };

            return
                project.ProjectItems
                    .OfType<ProjectItem>()
                    .SelectMany(p => GetProjects(p.SubProject));
        }
    }
}