using System;
using System.Collections.Generic;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.GraphModel.Schemas;
using NuGet.VisualStudio;

namespace Paket.VisualStudio.SolutionExplorer
{
    [GraphProvider(Name = "PaketGraphProvider")]
    public class PaketGraphProvider : IGraphProvider
    {
        private readonly List<IGraphContext> trackingContext = new List<IGraphContext>();

        [ImportingConstructor]
        public PaketGraphProvider(IShellPackage package)
        {
            new GraphIcons(package).Initialize();
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
                        var configNode = context.InputNodes.FirstOrDefault(node => node.IsPaketReferencesNode());
                        if (configNode != null)
                        {
                            using (var scope = new GraphTransactionScope())
                            {
                                configNode.SetValue(DgmlNodeProperties.ContainsChildren, true);
                                scope.Complete();
                            }
                        }

                        configNode = context.InputNodes.FirstOrDefault(node => node.IsPaketDependenciesNode());
                        if (configNode != null)
                        {
                            using (var scope = new GraphTransactionScope())
                            {
                                configNode.SetValue(DgmlNodeProperties.ContainsChildren, true);
                                scope.Complete();
                            }
                        }
                    }
                    break;
                }
                case GraphContextDirection.Contains:
                {
                    var configNode = context.InputNodes.FirstOrDefault(node => node.IsPaketReferencesNode());
                    if (configNode != null)
                    {
                        TryAddReferencesFilePackageNodes(context);
                    }

                    configNode = context.InputNodes.FirstOrDefault(node => node.IsPaketDependenciesNode());
                    if (configNode != null)
                    {
                        TryAddDependenciesFilePackageNodes(context);
                    }
                    break;
                }
                case GraphContextDirection.Custom:
                    break;
            }

            context.OnCompleted();
        }

        private void TryAddReferencesFilePackageNodes(IGraphContext context)
        {
            try
            {
                AddReferencesFilePackageNodes(context, context.InputNodes.First());
                TrackChanges(context);
            }
            catch { }
        }

        private void AddReferencesFilePackageNodes(IGraphContext context, GraphNode parentNode)
        {
            var file = parentNode.Id.GetNestedValueByName<Uri>(CodeGraphNodeIdName.File);

            if (file == null || !File.Exists(file.LocalPath))
                return;

            AddPackageNodes(context, () => parentNode, GetDependenciesFromReferencesFile(file.LocalPath));
        }

        private void TryAddDependenciesFilePackageNodes(IGraphContext context)
        {
            try
            {
                AddDependenciesFilePackageNodes(context, context.InputNodes.First());
                TrackChanges(context);
            }
            catch { }
        }


        private void AddDependenciesFilePackageNodes(IGraphContext context, GraphNode parentNode)
        {
            var file = parentNode.Id.GetNestedValueByName<Uri>(CodeGraphNodeIdName.File);

            if (file == null || !File.Exists(file.LocalPath))
                return;

            AddPackageNodes(context, () => parentNode, GetDependenciesFromFile(file.LocalPath));
        }


        private void AddPackageNodes(IGraphContext context, Func<GraphNode> parentNode, IEnumerable<IVsPackageMetadata> installedPackages)
        {
            var allPackages = installedPackages.ToList();

            for (int index = 0; index < allPackages.Count; index++)
            {
                var nugetPackage = allPackages[index];
                CreateNode(context, parentNode(), nugetPackage);

                context.ReportProgress(index, allPackages.Count, null);

                if (context.CancelToken.IsCancellationRequested)
                    break;
            }
        }

        private void CreateNode(IGraphContext context, GraphNode parent, IVsPackageMetadata metadata)
        {
            var parentId = parent.GetValue<GraphNodeId>("Id");
            var nodeId = GraphNodeId.GetNested(
                parentId,
                GraphNodeId.GetPartial(CodeGraphNodeIdName.Member, metadata.Id),
                GraphNodeId.GetPartial(CodeGraphNodeIdName.Parameter, metadata.VersionString));

            var node = context.Graph.Nodes.Get(nodeId);
            if (node == null)
            {
                using (var scope = new GraphTransactionScope())
                {
                    node = context.Graph.Nodes.GetOrCreate(nodeId, metadata.Id + " " + metadata.VersionString, PaketGraphSchema.PackageCategory);
                    node[DgmlNodeProperties.Icon] = GraphIcons.Package;
                    node[PaketGraphSchema.PackageProperty] = metadata;

                    // Establish the relationship with the parent node.
                    context.Graph.Links.GetOrCreate(parent, node, null, GraphCommonSchema.Contains);
                    context.OutputNodes.Add(node);
                    scope.Complete();
                }
            }
        }

        private IEnumerable<IVsPackageMetadata> GetDependenciesFromReferencesFile(string paketReferencesFile)
        {
            return Dependencies.Locate(paketReferencesFile)
                .GetDirectDependencies(ReferencesFile.FromFile(paketReferencesFile))
                .Select(d => new PaketMetadata(d.Item1, d.Item2));
        }

        private IEnumerable<IVsPackageMetadata> GetDependenciesFromFile(string paketDependenciesFile)
        {
            return DependenciesFile.ReadFromFile(paketDependenciesFile)
                                   .DirectDependencies
                                   .Select(d => new PaketMetadata(d.Key.Id, d.Value.ToString()));
        }

        private IEnumerable<IVsPackageMetadata> GetIndirectPackages(string paketReferencesFile, string packageName)
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
            if (typeof(T) == typeof(IGraphFormattedLabel))
            {
                // TODO: format labels dynamically?
            }
            else if (typeof(T) == typeof(IGraphNavigateToItem))
            {
                return new GraphNodeNavigator() as T;
            }

            return null;
        }

        public Graph Schema
        {
            get { return null; }
        }
    }
}
