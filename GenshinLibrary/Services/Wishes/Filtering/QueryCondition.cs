using Npgsql;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.Services.Wishes.Filtering
{
    public class QueryCondition
    {
        public string Conditions { get; }
        public IEnumerable<NpgsqlParameter> Parameters { get; }

        public bool IsEmpty => Conditions.Length == 0 || !Parameters.Any();

        public QueryCondition(string conditions, IEnumerable<NpgsqlParameter> parameters)
        {
            Conditions = conditions;
            Parameters = parameters;
        }
    }
}
