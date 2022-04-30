using GenshinLibrary.Utility;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GenshinLibrary.Services.Wishes.Filtering
{
    public interface IFilter
    {
        public string GetFilterCondition();
        public IEnumerable<NpgsqlParameter> GetParameters();
    }

    public class Filter<T> : IFilter
    {
        private static readonly string[] operators = { "!=", "=", ">", "<", ">=", "<=" };
        private static readonly string[] operatorsEquality = { "!=", "=" };
        private static readonly string[] operatorsInequality = { ">", "<", ">=", "<=" };

        private readonly string _name;
        private readonly ReadOnlyCollection<Constraint<T>> _constraints;
        private readonly FilterType _filterType;

        private Filter(string name, List<Constraint<T>> constraints, FilterType ft)
        {
            _name = name;
            _constraints = constraints.AsReadOnly();
            _filterType = ft;
        }

        public static Result<Filter<T>> TryParseFilter(string name, string rawConstraints, FilterType filterType = FilterType.Varying)
        {
            var split = rawConstraints.Split(',');
            var constraints = new List<Constraint<T>>();
            var operators = GetOperators(filterType);

            foreach (var strConstraint in split)
            {
                string op = null;
                foreach (var @operator in operators)
                    if (strConstraint.StartsWith(@operator))
                        op = @operator;

                if (op == null && filterType != FilterType.Inequality && TryParse(strConstraint, out var result1))
                {
                    constraints.Add(new Constraint<T>("=", result1));
                    continue;
                }

                if (op == null)
                    return new Result<Filter<T>>(null, false, $"Invalid operator. Only {string.Join(", ", operators.Select(x => $"`{x}`"))} are allowed as operators.");

                string value = strConstraint[op.Length..];
                if (TryParse(value, out var result2))
                    constraints.Add(new Constraint<T>(op, result2));
                else
                    return new Result<Filter<T>>(null, false, $"Invalid value for filter. `{value}` is not {typeof(T).Name}.");
            }

            if (filterType == FilterType.Varying)
                filterType = GetFilterType(constraints[0].Operator);

            foreach (var constraint in constraints)
                if (GetFilterType(constraint.Operator) != filterType)
                    return new Result<Filter<T>>(null, false, $"`{name}`: Cannot combine operator types. " +
                        $"Use either equality operators ({string.Join(", ", operatorsEquality.Select(x => $"`{x}`"))}) " +
                        $"or inequality operators ({string.Join(", ", operatorsInequality.Select(x => $"`{x}`"))})");

            if (constraints.Select(x => x.Value).ContainsDuplicates())
                return new Result<Filter<T>>(null, false, $"`{name}`: Duplicate values are not allowed.");

            if (constraints.Count > 3)
                return new Result<Filter<T>>(null, false, $"`{name}`: Only up to 3 constraints are allowed in a single filter.");

            return new Result<Filter<T>>(new Filter<T>(name, constraints, filterType), true, null);
        }

        public string GetFilterCondition()
        {
            StringBuilder builder = new StringBuilder();

            string logicOperator = _filterType switch
            {
                FilterType.Equality => " OR ",
                FilterType.Inequality => " AND ",
                _ => throw new NotImplementedException()
            };

            builder.Append("(");
            builder.Append(GetConstraintString(0, _constraints[0]));
            for (int i = 1; i < _constraints.Count; i++)
            {
                builder.Append(logicOperator);
                builder.Append(GetConstraintString(i, _constraints[i]));
            }
            builder.Append(")\n");

            return builder.ToString();
        }

        public IEnumerable<NpgsqlParameter> GetParameters()
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            for (int i = 0; i < _constraints.Count; i++)
                parameters.Add(new NpgsqlParameter($"{_name}{i}", _constraints[i].Value));
            return parameters;
        }

        private string GetConstraintString(int i, Constraint<T> constraint)
        {
            return $"{_name} {constraint.Operator} @{_name}{i}";
        }

        private static FilterType GetFilterType(string op)
        {
            if (operatorsEquality.Contains(op))
                return FilterType.Equality;
            else if (operatorsInequality.Contains(op))
                return FilterType.Inequality;
            else
                throw new ArgumentException("Invalid operator.");
        }

        private static string[] GetOperators(FilterType type)
        {
            return type switch
            {
                FilterType.Equality => operatorsEquality,
                FilterType.Inequality => operatorsInequality,
                FilterType.Varying => operators,
                _ => throw new NotImplementedException()
            };
        }

        private static bool TryParse(string input, out T result)
        {
            bool isConversionSuccessful = false;
            result = default;

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    result = (T)converter.ConvertFromString(input);
                    isConversionSuccessful = true;
                }
                catch { }
            }

            return isConversionSuccessful;
        }
    }
}
