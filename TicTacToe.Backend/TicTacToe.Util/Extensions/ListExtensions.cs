using System;
using System.Collections.Generic;
using System.Text;

namespace Next_V4.Util.Extensions
{
    public static class ListExtensions
    {

        public static void ForEach<T>(this IEnumerable<T> itens, Action<T> action)
        {
            if (itens == null) return;
            foreach (var item in itens)
            {
                action(item);
            }
        }

        public static IEnumerable<TResult> Map<T, TResult>(this IEnumerable<T> itens, Func<T, TResult> transform)
        {
            foreach (var item in itens)
            {
                yield return transform(item);
            }
        }

    }
}
