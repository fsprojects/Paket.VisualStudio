using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.GraphModel.CodeSchema;
using Microsoft.VisualStudio.GraphModel.Schemas;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;

namespace Paket.VisualStudio.SolutionExplorer
{
    [GraphProvider(Name = "PaketGraphProvider")]
    public class PaketGraphProvider : IGraphProvider
    {
        private readonly List<IGraphContext> trackingContext = new List<IGraphContext>();

        [ImportingConstructor]
        public PaketGraphProvider(SVsServiceProvider serviceProvider)
        {
            new GraphIcons(serviceProvider).Initialize();
        }

        public void BeginGetGraphData(IGraphContext context)
        {
            if (context.CancelToken.IsCancellationRequested)
                return;

            switch (context.Direction)
            {
                case GraphContextDirection.Self:
                {
                    if (context.RequestedProperties.Contains(DgmlNodeProperties.ContainsChildren))
                    {
                        Prepare(context);
                    }
                    break;
                }
                case GraphContextDirection.Contains:
                {
                    Prepare(context);
                    TryPopulatePaketNode(context);
                    break;
                }
                case GraphContextDirection.Custom:
                    break;
            }

            context.OnCompleted();
        }

        private void Prepare(IGraphContext context)
        {
            GraphNode selectedNode = context.InputNodes.FirstOrDefault(node => node.IsPaketDependenciesNode() || node.IsPaketReferencesNode());
            if (selectedNode == null)
                return;

            using (GraphTransactionScope transactionScope = new GraphTransactionScope())
            {
                selectedNode.SetValue(DgmlNodeProperties.ContainsChildren, true);
                selectedNode.AddCategory(PaketGraphSchema.PaketCategory);
                transactionScope.Complete();
            }
        }

        private void TryPopulatePaketNode(IGraphContext context)
        {
            try
            {
                CreateChildNodes(context, context.InputNodes.First());
                TrackChanges(context);
            }
            catch 
            { 
                // todo logging 
            }
        }

        private void CreateChildNodes(IGraphContext context, GraphNode parentNode)
        {
            Uri file = parentNode.Id.GetNestedValueByName<Uri>(CodeGraphNodeIdName.File);

            if (file == null || !File.Exists(file.LocalPath))
                return;

            if (parentNode.IsPaketDependenciesNode())
            {
                AddPackageNodes(context, () => parentNode, GetDependenciesFromFile(file.LocalPath));
            } 
            else if (parentNode.IsPaketReferencesNode())
            {
                AddPackageNodes(context, () => parentNode, GetDependenciesFromReferencesFile(file.LocalPath));
            }
        }

        private void AddPackageNodes(IGraphContext context, Func<GraphNode> parentNode, IEnumerable<PaketMetadata> installedPackages)
        {
            var allPackages = installedPackages.ToList();

            for (int index = 0; index < allPackages.Count; index++)
            {
                PaketMetadata paketMetadata = allPackages[index];

                using (var scope = new GraphTransactionScope())
                {
                    CreateNode(context, parentNode(), paketMetadata);
                    scope.Complete();
                }

                context.ReportProgress(index, allPackages.Count, null);

                if (context.CancelToken.IsCancellationRequested)
                    break;
            }
        }

        private void CreateNode(IGraphContext context, GraphNode parent, PaketMetadata metadata)
        {
            var parentId = parent.GetValue<GraphNodeId>("Id");
            var id = GraphNodeId.GetNested(
                parentId,
                GraphNodeId.GetPartial(CodeGraphNodeIdName.Member, metadata.Id),
                GraphNodeId.GetPartial(CodeGraphNodeIdName.Parameter, metadata.VersionString));

            var node = context.Graph.Nodes.Get(id);
            if (node == null)
            {
                string label = metadata.Id + " " + metadata.VersionString;

                node = context.Graph.Nodes.GetOrCreate(id, label, PaketGraphSchema.PaketCategory);

                node.SetValue(DgmlNodeProperties.ContainsChildren, false);
                node.SetValue(CodeNodeProperties.SourceLocation, new SourceLocation(new Uri(parentId.GetFileName(), UriKind.Absolute),
                                                                 new Position(PaketGraphSchema.GetDisplayIndex(node), 1)));
                node.SetValue(DgmlNodeProperties.Icon, GraphIcons.Package);
            }

            context.Graph.Links.GetOrCreate(parent, node, null, GraphCommonSchema.Contains);
        }

        private IEnumerable<PaketMetadata> GetDependenciesFromReferencesFile(string paketReferencesFile)
        {
            return Dependencies.Locate(paketReferencesFile)
                .GetDirectDependencies(ReferencesFile.FromFile(paketReferencesFile))
                .Select(d => new PaketMetadata(d.Item1, d.Item2));
        }

        private IEnumerable<PaketMetadata> GetDependenciesFromFile(string paketDependenciesFile)
        {
            return DependenciesFile.ReadFromFile(paketDependenciesFile)
                                   .DirectDependencies
                                   .Select(d => new PaketMetadata(d.Key.Id, d.Value.ToString()));
        }

        private IEnumerable<PaketMetadata> GetIndirectPackages(string paketReferencesFile, string packageName)
        {
            return Dependencies.Locate(paketReferencesFile)
                    .GetDirectDependenciesForPackage(packageName)
                    .Select(d => new PaketMetadata(d.Item1, d.Item2));
        }

        private void TrackChanges(IGraphContext context)
        {
            if (!trackingContext.Contains(context))
            {
                context.Canceled += OnContextCanceled;
                trackingContext.Add(context);
            }
        }

        private void OnContextCanceled(object sender, EventArgs e)
        {
            var context = sender as IGraphContext;
            if (trackingContext.Contains(context))
            {
                trackingContext.Remove(context);
            }
        }

        public IEnumerable<GraphCommand> GetCommands(IEnumerable<GraphNode> nodes)
        {
            return new[]
            {
                new GraphCommand(GraphCommandDefinition.Contains, null, null, true)
            };
        }

        public T GetExtension<T>(GraphObject graphObject, T previous) where T : class
        {
            //if (typeof(T) == typeof(IGraphFormattedLabel))
            //{
            //    return null;
            //}
            //if (typeof(T) == typeof(IGraphNavigateToItem))
            //{
            //    return null;
            //}

            return null;
        }

        public Graph Schema
        {
            get { return null; }
        }
    }
}
