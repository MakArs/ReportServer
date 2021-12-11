using System;
using System.Collections.Generic;

namespace ReportService.Extensions
{
    public static class ExceptionExtensions //can be used not only for errors
    {
        public static IEnumerable<Exception> GetExceptionTree(this Exception source)
        {
            return FromHierarchy(source, ex => ex.InnerException, s => s != null);
        }

        private static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }
    }
}
