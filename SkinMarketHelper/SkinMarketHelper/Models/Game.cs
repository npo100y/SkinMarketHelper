using System.Collections.Generic;

namespace SkinMarketHelper.Models
{
    public class Game
    {
        public int GameId { get; set; }
        public string Name { get; set; }
        public int AppId { get; set; }
        public string LogoUrl { get; set; }

        public ICollection<Item> Items { get; set; }
    }
}
