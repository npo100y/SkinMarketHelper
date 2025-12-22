using System.ComponentModel.DataAnnotations.Schema;

namespace SkinMarketHelper.Models
{
    public partial class MarketListing
    {
        [NotMapped]
        public bool IsInCurrentUserCart { get; set; }
        [NotMapped]
        public bool IsOwnedByCurrentUser { get; set; }
    }
}
