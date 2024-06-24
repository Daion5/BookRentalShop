using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookRentalShop.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 30 characters long")]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s]+$", ErrorMessage = "Name can only contain letters and spaces")]

        public string Name { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}