using System;
using System.Collections.Generic;

namespace Paket.VisualStudio.Utils
{
    internal static class ListHelpers
    {
        public static void RemoveDuplicates<T>(this List<T> list) where T : IComparable<T>
        {
            if (list == null)
                throw new ArgumentNullException("list");

            list.Sort();

            int next = 0;
            for (int index = 1; index < list.Count; ++index)
            {
                if (list[index].CompareTo(list[next]) != 0)
                    list[++next] = list[index];
            }

            int k = next + 1;
            if (k >= list.Count)
                return;

            list.RemoveRange(k, list.Count - k);
        }
    }
}