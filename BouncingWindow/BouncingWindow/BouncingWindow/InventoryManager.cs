using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncingWindow
{
    public static class InventoryManager
    {
        private static Dictionary<string, int> items = new();

        public static void AddItem(string name, int quantity)//adds an item and its quantity to the inventory
        {
            if (items.ContainsKey(name))
                items[name] += quantity;
            else
                items[name] = quantity;
        }

        public static Dictionary<string, int> GetItems()//getter
        {
            return items;
        }

        public static void Clear()
        {
            items.Clear();
        }
    }
}