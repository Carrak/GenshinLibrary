using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenshinLibrary.StringTable
{
    class TextTable
    {
        public string Format { get; set; } = "```inform7\n{0}\n```";

        public readonly List<string[]> Rows = new List<string[]>();
        private readonly string[] _columns;

        public TextTable(params string[] columns)
        {
            if (columns.Length == 0)
                throw new Exception("Cannot create a table with no columns.");

            _columns = columns;
        }

        public void AddRow(params string[] row)
        {
            if (row.Length != _columns.Length)
                throw new Exception("Row must have the same amount of columns as the table.");

            Rows.Add(row);
        }

        public int GetLength()
        {
            return GetLengths().Sum() + _columns.Length + 1;
        }

        public int[] GetLengths()
        {
            int columnCount = _columns.Length;
            int[] maxLengths = new int[columnCount];

            for (int i = 0; i < columnCount; i++)
                maxLengths[i] = _columns[i].Length;

            for (int i = 0; i < Rows.Count; i++)
                for (int j = 0; j < columnCount; j++)
                    maxLengths[j] = Math.Max(Rows[i][j].Length, maxLengths[j]);

            return maxLengths;
        }

        public string GetTable()
        {
            int columnCount = _columns.Length;
            int[] maxLengths = GetLengths();

            StringBuilder result = new StringBuilder("");
            for (int i = 0; i < columnCount; i++)
                result.Append(string.Format("|{0, -" + maxLengths[i] + "}", _columns[i]));
            result.Append("|\n");

            for (int i = 0; i < columnCount; i++)
            {
                result.Append("+");
                result.Append('-', maxLengths[i]);
            }
            result.Append("+\n");


            for (int i = 0; i < Rows.Count; i++)
            {
                var row = Rows[i];
                string toAdd = "";

                for (int j = 0; j < columnCount; j++)
                    toAdd += string.Format("|{0, -" + maxLengths[j] + "}", row[j]);
                toAdd += "|\n";
                result.Append(toAdd);
            }

            return string.Format(Format, result.ToString());
        }
    }
}