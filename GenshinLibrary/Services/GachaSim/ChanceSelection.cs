using System;
using System.Collections.Generic;

namespace GenshinLibrary.Services.GachaSim
{
    public class ChanceSelection<T>
    {
        private IEnumerable<ChanceSelectionItem<T>> _items { get; }

        public ChanceSelection(params ChanceSelectionItem<T>[] items)
        {
            _items = items;
        }

        public T GetValue(double num)
        {
            double low = 0;
            foreach (var item in _items)
            {
                if (num >= low && num < low + item.Probability)
                    return item.Value;
                low += item.Probability;
            }

            throw new Exception("Incorrect probability values. Must add up to at least 1.");
        }
    }

    public class ChanceSelectionItem<T>
    {
        public double Probability;
        public T Value { get; }

        public ChanceSelectionItem(double probability, T value)
        {
            Probability = probability;
            Value = value;
        }
    }
}
