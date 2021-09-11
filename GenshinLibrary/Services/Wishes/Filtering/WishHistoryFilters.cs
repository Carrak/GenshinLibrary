using GenshinLibrary.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace GenshinLibrary.Services.Wishes.Filtering
{
    public class WishHistoryFilters
    {
        public Filter<int> RarityFilter { get; }
        public Filter<DateTime> DateTimeFilter { get; }
        public Filter<string> NameFilter { get; }
        public Filter<int> PityFilter { get; }
        public string Order { get; }
        public bool SeparatePity { get; }

        public WishHistoryFilters(WishHistoryFilterValues values)
        {
            if (values.Rarity != null)
                RarityFilter = new Filter<int>("rarity", values.Rarity, FilterType.Equality);

            if (values.DateTime != null)
                DateTimeFilter = new Filter<DateTime>("datetime", values.DateTime, FilterType.Inequality);

            if (values.Name != null)
                NameFilter = new Filter<string>("name", values.Name, FilterType.Equality);

            if (values.Pity != null)
                PityFilter = new Filter<int>("pity", values.Pity, FilterType.Varying);

            Order = values.Order;
            SeparatePity = values.Sp;
        }

        public QueryCondition GetCondition()
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            List<string> conditions = new List<string>();

            if (RarityFilter != null)
            {
                conditions.Add(RarityFilter.GetFilterCondition());
                parameters.AddRange(RarityFilter.GetParameters());
            }

            if (DateTimeFilter != null)
            {
                conditions.Add(DateTimeFilter.GetFilterCondition());
                parameters.AddRange(DateTimeFilter.GetParameters());
            }

            if (NameFilter != null)
            {
                conditions.Add(NameFilter.GetFilterCondition());
                parameters.AddRange(NameFilter.GetParameters());
            }

            if (PityFilter != null)
            {
                conditions.Add(PityFilter.GetFilterCondition());
                parameters.AddRange(PityFilter.GetParameters());
            }

            string order = null;
            if (Order != null)
            {
                if (Order != "pity" && Order != "rarity")
                    throw new ArgumentException("Can only order by `pity` or `rarity`");

                order = $"ORDER BY {Order} DESC";
            }

            return new QueryCondition($"\nWHERE {string.Join(" AND ", conditions)}\n{order}", parameters);
        }
    }
}
