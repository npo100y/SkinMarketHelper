using System.Collections.Generic;

namespace SkinMarketHelper.Models
{
    public class Item
    {
        public int ItemId { get; set; }
        public int GameId { get; set; }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rarity { get; set; }
        public string IconUrl { get; set; }
        public string SteamMarketHashName { get; set; }

        public Game Game { get; set; }
        public ICollection<PriceListing> PriceListings { get; set; }
        public ICollection<UserInventoryItem> UserInventoryItems { get; set; }
    }
}
