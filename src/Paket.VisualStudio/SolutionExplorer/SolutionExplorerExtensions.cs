﻿using System;
using System.Linq;

using Microsoft.VisualStudio;
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

        private static string GetPaketDirectory(DirectoryInfo current, DirectoryInfo sln)
        {
            var paketFolder = new DirectoryInfo(Path.Combine(current.FullName, ".paket"));
            if (paketFolder.Exists)
                return current.FullName;
            var depsFile = new FileInfo(Path.Combine(current.FullName, "paket.dependencies"));
            if (depsFile.Exists)
                return current.FullName;
            if (current.Parent == null)
                return sln.FullName;
            return GetPaketDirectory(current.Parent, sln);
        }

        public static string GetPaketDirectory()
        {
            var di = new DirectoryInfo(SolutionExplorerExtensions.GetSolutionDirectory());
            return GetPaketDirectory(di,di);
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

            foreach (Project project in GetAllProjects())
            {
                solution.GetProjectOfUniqueName(GetProjectFullName(project), out hierarchy);

                if (hierarchy != null)
                {
                    Guid projectGuid = Guid.Empty;
                    solution.GetGuidOfProject(hierarchy, out projectGuid);
                    projectGuids.Add(projectGuid);
                }
            }
            return projectGuids;
        }

        private static string GetProjectFullName(Project project)
        {
            var solutionFolder = Path.GetDirectoryName(DteUtils.DTE.Solution.FullName);
            return Path.Combine(solutionFolder, project.UniqueName);
        }

        public static IEnumerable<Project> GetAllProjects()
        {
            return DteUtils.DTE.Solution.Projects
                .OfType<Project>()
                .SelectMany(GetProjects)
                .Where(p => File.Exists(GetProjectFullName(p)));
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
