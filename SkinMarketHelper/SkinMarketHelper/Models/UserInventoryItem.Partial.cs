using System.ComponentModel.DataAnnotations.Schema;

namespace SkinMarketHelper.Models
{
    public partial class UserInventoryItem
    {
        [NotMapped]
        public bool IsOnSale { get; set; }
    }
}
