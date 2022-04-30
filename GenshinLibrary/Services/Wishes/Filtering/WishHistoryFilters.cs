using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.Services.Wishes.Filtering
{
    public class WishHistoryFilters
    {
        private readonly IEnumerable<IFilter> _filters;

        public WishHistoryFilters(IEnumerable<IFilter> filters)
        {
            _filters = filters;
        }

        public static Result<WishHistoryFilters> Parse(string rarityFilterRaw, string dateTimeFilterRaw, string nameFilterRaw, string pityFilterRaw)
        {
            IEnumerable<Result<IFilter>> results = new List<Result<IFilter>>()
            {
                ParseFilter<int>("rarity", rarityFilterRaw, FilterType.Equality),
                ParseFilter<DateTime>("datetime", dateTimeFilterRaw, FilterType.Inequality),
                ParseFilter<string>("name", nameFilterRaw, FilterType.Equality),
                ParseFilter<int>("pity", pityFilterRaw, FilterType.Varying)
            };

            results = results.Where(x => x != null);
            foreach (var result in results)
                if (!result.IsSuccess)
                    return new Result<WishHistoryFilters>(null, false, result.ErrorMessage);

            return new Result<WishHistoryFilters>(new WishHistoryFilters(results.Select(x => x.Value)), true, null);
        }

        private static Result<IFilter> ParseFilter<T>(string name, string raw, FilterType filterType)
        {
            if (string.IsNullOrEmpty(raw))
                return null;

            var filterResult = Filter<T>.TryParseFilter(name, raw, filterType);
            if (filterResult.IsSuccess)
                return new Result<IFilter>(filterResult.Value, true, null);
            else
                return new Result<IFilter>(null, false, filterResult.ErrorMessage);
        }

        public QueryCondition GetCondition()
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            List<string> conditions = new List<string>();

            foreach (var filter in _filters)
            {
                conditions.Add(filter.GetFilterCondition());
                parameters.AddRange(filter.GetParameters());
            }

            return new QueryCondition($"\nWHERE {string.Join(" AND ", conditions)}", parameters);
        }
    }
}
