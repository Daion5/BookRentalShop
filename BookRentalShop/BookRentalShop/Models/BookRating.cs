using System.ComponentModel.DataAnnotations;

namespace BookRentalShop.Models
{
    public class BookRating
    {
        public int BookRatingId { get; set; }
        public int BookId { get; set; }
        public virtual Book Book { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}