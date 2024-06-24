using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BookRentalShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<OverdueFee> OverdueFees { get; set; } = new List<OverdueFee>();
    }
}
