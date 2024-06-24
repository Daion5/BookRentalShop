using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookRentalShop.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 80 characters long")]
        [RegularExpression(@"^[-,.a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s]+$", ErrorMessage = "Title can only contain letters and spaces")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Author must be between 1 and 80 characters long")]
        [RegularExpression(@"^[-a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s]+$", ErrorMessage = "Author can only contain letters and spaces")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1.00, 1000.00, ErrorMessage = "Price must be between 1.00 and 1,000.00")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(1455, 2024, ErrorMessage = "Year must be between 1455 and the current year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters long")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Available copies are required")]
        [Range(0, 1000, ErrorMessage = "Available copies must be between 0 and 1000")]
        public int AvailableCopies { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<BookRating> Ratings { get; set; }
    }
}
