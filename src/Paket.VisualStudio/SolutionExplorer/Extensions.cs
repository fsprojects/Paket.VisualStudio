using System.Collections.Generic;
using System.Linq;

namespace Paket.VisualStudio.SolutionExplorer
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
        {
            return from x in source.Select((x, i) => new
            {
                Index = i,
                Value = x
            })
                   group x by x.Index / size
                       into x
                       select (from v in x
                               select v.Value).ToList<T>();
        }
    }
}