using Debex.Convert.Data;
using System.Collections.Generic;
using System.Linq;

namespace Debex.Convert.Extensions
{
    public static class Enumerables
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> arr) => !arr?.Any() ?? true;

        public static T ById<T>(this IEnumerable<T> vals, int id) where T : IHasId => vals.First(x => x.Id == id);

        public static BaseFieldToMatch MatchedById(this IEnumerable<BaseFieldToMatch> vals, int id)
        {
            var field = vals.ById(id);
            return field.IsMatched ? field : null;
        }

        public static List<BaseFieldToMatch> MatchedByIds(this IEnumerable<BaseFieldToMatch> vals, params int[] ids) =>
            ids
                .Select(vals.MatchedById)
                .Where(x => x != null)
                .ToList();

        public static bool MatchedAll(this IEnumerable<BaseFieldToMatch> vals, params int[] ids) => ids.All(x => vals.First(v => v.Id == x).IsMatched);
        public static bool MatchedAny(this IEnumerable<BaseFieldToMatch> vals, params int[] ids) => ids.Any(x => vals.ById(x).IsMatched);

        public static bool AnyIsNull<T>(this IEnumerable<T> vals) => vals.Any(x => x == null);
    }

}
