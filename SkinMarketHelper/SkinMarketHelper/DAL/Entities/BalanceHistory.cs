namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BalanceHistory")]
    public partial class BalanceHistory
    {
        public int BalanceHistoryID { get; set; }

        public int UserID { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? CreatedAt { get; set; }

        public virtual Users Users { get; set; }
    }
}
