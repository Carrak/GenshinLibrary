using GenshinLibrary.Utility;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GenshinLibrary.Services.Wishes.Filtering
{
    public class Filter<T>
    {
        private static readonly string[] operators = { "!=", "=", ">", "<", ">=", "<=" };
        private static readonly string[] operatorsEquality = { "!=", "=" };
        private static readonly string[] operatorsInequality = { ">", "<", ">=", "<=" };

        public string Name { get; }
        public readonly List<Constraint<T>> Constraints = new List<Constraint<T>>();

        private FilterType _filterType;

        public Filter(string name, string constraints, FilterType filterType = FilterType.Varying)
        {
            _filterType = filterType;
            Name = name;
            ParseConstraints(constraints);
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
            builder.Append(GetConstraintString(0, Constraints[0]));
            for (int i = 1; i < Constraints.Count; i++)
            {
                builder.Append(logicOperator);
                builder.Append(GetConstraintString(i, Constraints[i]));
            }
            builder.Append(")\n");

            return builder.ToString();
        }

        public IEnumerable<NpgsqlParameter> GetParameters()
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            for (int i = 0; i < Constraints.Count; i++)
                parameters.Add(new NpgsqlParameter($"{Name}{i}", Constraints[i].Value));
            return parameters;
        }

        private void ParseConstraints(string input)
        {
            var split = input.Split(',');

            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException("No constraints have been provided.");

            foreach (var stringConstraint in split)
                Constraints.Add(ParseConstraint(stringConstraint));

            if (_filterType == FilterType.Varying)
                _filterType = GetFilterType(Constraints[0].Operator);

            var operators = GetOperators(_filterType);
            foreach (var constraint in Constraints)
                if (GetFilterType(constraint.Operator) != _filterType)
                    throw new ArgumentException($"`{Name}`: Cannot combine operator types. " +
                        $"Use either equality operators ({string.Join(", ", operatorsEquality.Select(x => $"`{x}`"))}) " +
                        $"or inequality operators ({string.Join(", ", operatorsInequality.Select(x => $"`{x}`"))})");

            if (Constraints.Select(x => x.Value).ContainsDuplicates())
                throw new ArgumentException($"`{Name}`: Duplicate values are not allowed.");

            if (Constraints.Count > 3)
                throw new ArgumentException($"`{Name}`: Only up to 3 constraints are allowed in a single filter.");
        }

        private Constraint<T> ParseConstraint(string input)
        {
            var operators = GetOperators(_filterType);

            string op = null;
            foreach (var @operator in operators)
                if (input.StartsWith(@operator))
                    op = @operator;

            if (op == null && _filterType != FilterType.Inequality && TryParse(input, out var result1))
                return new Constraint<T>("=", result1);

            if (op == null)
                throw new ArgumentException($"Invalid operator. Only {string.Join(", ", operators.Select(x => $"`{x}`"))} are allowed as operators.");

            string value = input.Substring(op.Length);
            if (TryParse(value, out var result2))
                return new Constraint<T>(op, result2);
            else
                throw new ArgumentException($"Invalid value for filter. `{value}` is not {typeof(T).Name}.");
        }

        private FilterType GetFilterType(string op)
        {
            if (operatorsEquality.Contains(op))
                return FilterType.Equality;
            else if (operatorsInequality.Contains(op))
                return FilterType.Inequality;
            else
                throw new ArgumentException("Invalid operator.");
        }

        private string[] GetOperators(FilterType type)
        {
            return type switch
            {
                FilterType.Equality => operatorsEquality,
                FilterType.Inequality => operatorsInequality,
                FilterType.Varying => operators,
                _ => throw new NotImplementedException()
            };
        }

        private string GetConstraintString(int i, Constraint<T> constraint)
        {
            return $"{Name} {constraint.Operator} @{Name}{i}";
        }

        private bool TryParse(string input, out T result)
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
