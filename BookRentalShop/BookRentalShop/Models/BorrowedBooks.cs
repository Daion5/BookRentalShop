using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookRentalShop.Models
{
    public class BorrowedBooks
    {
        [Key]
        public int BorrowedBookId { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal OverdueFee { get; set; }

        public void CalculateOverdueFee()
        {
            DateTime dueDate = BorrowDate;
            int overdueDays = (DateTime.Now - dueDate).Days;
            OverdueFee = overdueDays > 0 ? overdueDays * 5 : 0;
        }
    }
}
