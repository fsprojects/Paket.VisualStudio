using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.GraphModel.Schemas;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal static class PaketGraphSchema
    {
        private static readonly GraphSchema Schema = new GraphSchema("Paket");
        public static readonly GraphCategory PackageCategory = Schema.Categories.AddNewCategory("NuGetPackage");
        public static readonly GraphProperty PackageProperty = Schema.Properties.AddNewProperty("NuGetPackageProperty", typeof(NuGet.VisualStudio.IVsPackageMetadata));
    }

    internal static class GraphNodeExtensions
    {
        internal static bool IsPaketDependenciesNode(this GraphNode node)
        {
            return node.HasCategory(CodeNodeCategories.ProjectItem) && node.Label == "paket.dependencies";
        }
    }
}