using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Extensions
{
    public static class Tasks
    {
        public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) => Task.WhenAll(tasks);
    }
}
