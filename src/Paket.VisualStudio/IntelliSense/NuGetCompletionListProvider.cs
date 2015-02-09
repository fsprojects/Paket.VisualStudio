using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense
{
    internal class NuGetCompletionListProvider : ICompletionListProvider
    {
        private static readonly Regex r = new Regex("nuget (?<query>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CompletionContextType ContextType
        {
            get { return CompletionContextType.NuGet; }
        }

        public IEnumerable<Completion> GetCompletionEntries(CompletionContext context)
        {
            string text = context.Snapshot.GetText(context.SpanStart, context.SpanLength);
            string query = r.Match(text).Groups["query"].Value;

            // todo
            if (query.Length < 3)
                return Enumerable.Empty<Completion>();

            string[] results = FSharpAsync.StartAsTask(
                NuGetV3.FindPackages(FSharpOption<Paket.Utils.Auth>.None, "http://nuget.org/api/v2", query),
                new FSharpOption<TaskCreationOptions>(TaskCreationOptions.None),
                new FSharpOption<CancellationToken>(CancellationToken.None)).Result;

            return results.Select(item => new Completion2(item, item, null, null, item));
        }
    }
}