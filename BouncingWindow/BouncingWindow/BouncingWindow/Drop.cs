using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace BouncingWindow
{
    public class Drop //Constructor for drop class
    {

        //getters and setters
        public string Item { get; set; }
        public string Quantity { get; set; }
        public string Rarity { get; set; }
        public string Price { get; set; }
        public string HighAlch { get; set; }

        public int GetRandomQuantity()//THis class takes item quantity and parses it to get a realistic number
        {
            string cleanQty = HtmlEntity.DeEntitize(Quantity)
                .Replace("(noted)", "")
                .Trim()
                .Replace("–", "-") // en dash
                .Replace("—", "-"); // em dash

            if (cleanQty.Contains("-"))
            {
                var parts = cleanQty.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), out int min) &&
                    int.TryParse(parts[1].Trim(), out int max))
                {
                    return new Random().Next(min, max + 1);
                }
            }

            if (int.TryParse(cleanQty, out int fixedQty))
                return fixedQty;

            return 1;
        }
    }

    public static class DropFetcher//gets a drop from the loot table when the balloon pops
    {
        private static List<Drop>? cachedDrops = null;

        public static async Task<List<Drop>> GetDropsAsync()
        {
            if (cachedDrops != null)
                return cachedDrops;

            cachedDrops = await FetchDragonImplingDropsAsync();
            return cachedDrops;
        }

        private static async Task<List<Drop>> FetchDragonImplingDropsAsync()//scrapes the wiki api for the loot table then converts it into a list of drops
        {
            var drops = new List<Drop>();
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            string apiUrl = "https://oldschool.runescape.wiki/api.php?action=parse&page=Dragon_impling&format=json";

            string response = await client.GetStringAsync(apiUrl);
            using var doc = JsonDocument.Parse(response);
            var html = doc.RootElement.GetProperty("parse").GetProperty("text").GetProperty("*").GetString();

            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);

            var dropTable = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class,'item-drops')]");
            if (dropTable != null)
            {
                var rows = dropTable.SelectNodes(".//tr[position()>1]");
                foreach (var row in rows)
                {
                    var cols = row.SelectNodes("td");
                    if (cols != null && cols.Count >= 6)
                    {
                        drops.Add(new Drop
                        {
                            Item = HtmlEntity.DeEntitize(cols[1].InnerText.Trim()),
                            Quantity = cols[2].InnerText.Trim(),
                            Rarity = cols[3].InnerText.Trim(),
                            Price = cols[4].InnerText.Trim(),
                            HighAlch = cols[5].InnerText.Trim()
                        });
                    }
                }
            }

            //write to log
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drops_log.txt");
            using (StreamWriter writer = new StreamWriter(logPath, append: false))
            {
                foreach (var drop in drops)
                {
                    writer.WriteLine($"{drop.Item} | Qty: {drop.Quantity} | Rarity: {drop.Rarity} | GE: {drop.Price} | Alch: {drop.HighAlch}");
                }
            }

            return drops;
        }

        public static Drop GetRandomDrop(List<Drop> drops)//this takes into account the rarity of a drop when picking a drop
        {
            Random rand = new Random();
            var weightedList = new List<Drop>();

            foreach (var drop in drops)
            {
                string[] parts = drop.Rarity.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[1], out int denominator))
                {
                    int weight = 100 / denominator;
                    for (int i = 0; i < weight; i++)
                    {
                        weightedList.Add(drop);
                    }
                }
            }

            if (weightedList.Count == 0) return null;
            return weightedList[rand.Next(weightedList.Count)];
        }
    }
}