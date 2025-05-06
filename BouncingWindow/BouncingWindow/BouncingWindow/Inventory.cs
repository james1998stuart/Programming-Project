using System.Collections.Generic;
using System.Text;

namespace BouncingWindow
{
    public static class Inventory
    {
        private static Dictionary<string, int> inventory = new();
        private static Dictionary<string, int> items = new();
        public static void Add(string item, int quantity)//adds quantity of an item to the inventory
        {
            if (inventory.ContainsKey(item))
                inventory[item] += quantity;
            else
                inventory[item] = quantity;
        }

        public static string GetSortedInventoryText()//returns a string representation of the inventory sorted by quantity
        {
            var sorted = new List<KeyValuePair<string, int>>(items);

            // Bubble Sort by quantity (descending)
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                for (int j = 0; j < sorted.Count - i - 1; j++)
                {
                    if (sorted[j].Value < sorted[j + 1].Value)
                    {
                        var temp = sorted[j];
                        sorted[j] = sorted[j + 1];
                        sorted[j + 1] = temp;
                    }
                }
            }

            return string.Join(Environment.NewLine, sorted.Select(kvp => $"{kvp.Key} x{kvp.Value}"));//converts each value from the dictionary into a string and combines them into a multiline string
        }

        public static void AddItem(string itemName, int quantity)//adds an item to the inventory
        {
            if (items.ContainsKey(itemName))
                items[itemName] += quantity;
            else
                items[itemName] = quantity;
        }

        public static string GetInventorySummary()//returns a string representation of the inventory
        {
            var sb = new StringBuilder();
            foreach (var entry in inventory)
            {
                sb.AppendLine($"{entry.Key} x{entry.Value}");
            }
            return sb.ToString();
        }

        public static string GetInventoryText()//builds and returns a string representation of the inventory
        {
            if (items.Count == 0)
                return "Inventory is empty.";

            StringBuilder sb = new StringBuilder();
            foreach (var entry in items)
            {
                sb.AppendLine($"{entry.Key} x{entry.Value}");
            }
            return sb.ToString();
        }
    }
}