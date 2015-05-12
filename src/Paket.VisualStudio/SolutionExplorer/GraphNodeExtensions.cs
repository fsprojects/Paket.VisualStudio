using System;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.GraphModel.Schemas;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal static class GraphNodeExtensions
    {
        internal static bool IsPaketReferencesNode(this GraphNode node)
        {
            return node.HasCategory(CodeNodeCategories.ProjectItem) && node.Label == Paket.Constants.ReferencesFile;
        }

        internal static bool IsPaketDependenciesNode(this GraphNode node)
        {
            return node.HasCategory(CodeNodeCategories.ProjectItem) && node.Label == Paket.Constants.DependenciesFileName;
        }

        internal static string GetFileName(this GraphNodeId nodeId)
        {
            Uri fileName = nodeId.GetNestedValueByName<Uri>(CodeGraphNodeIdName.File);            

            if (fileName != null) 
                return fileName.LocalPath;

            var start = nodeId.LiteralValue.IndexOf("File=file:///") + 13;
            if (start < 0) return null;
            var end = nodeId.LiteralValue.IndexOf(')',start);
            if (end < 0) return null;
            return nodeId.LiteralValue.Substring(start, end - start);
        }

        internal static string GetPackageName(this GraphNode node)
        {
            return node.Label.Split(' ')[0];
        }
    }
}