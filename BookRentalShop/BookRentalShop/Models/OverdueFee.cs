using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookRentalShop.Models
{
    public class OverdueFee
    {
        [Key]
        public int OverdueFeeId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
