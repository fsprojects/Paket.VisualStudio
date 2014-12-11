using System.Linq;
using Microsoft.VisualStudio.GraphModel;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal static class PaketGraphSchema
    {
        private static readonly GraphSchema Schema = new GraphSchema("Paket");

        public static readonly GraphCategory PaketCategory = Schema.Categories.AddNewCategory("PaketCategory");
        public static readonly GraphProperty PaketProperty = Schema.Properties.AddNewProperty("PaketProperty", typeof(PaketMetadata));

        internal static int GetDisplayIndex(GraphNode node)
        {
            GraphCategory graphCategory = node.Categories.FirstOrDefault();
            if (graphCategory == null)
                return int.MaxValue;
            if (graphCategory.Id == PaketCategory.Id)
                return 1;

            return int.MaxValue;
        }
    }
}