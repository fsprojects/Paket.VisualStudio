using Microsoft.VisualStudio.GraphModel;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal class GraphNodeNavigator : IGraphNavigateToItem
    {
        public void NavigateTo(GraphObject graphObject)
        {
            // TODO: implement navigating to paket.dependencies
        }

        public int GetRank(GraphObject graphObject)
        {
            return GraphNavigateToItemRanks.CanNavigateToItem;
        }
    }
}