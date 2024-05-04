using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Login.Models
{
    public class User
    {
        [ValidateNever]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 8)]
        public string? Password { get; set; }

        public ICollection<Contact>? Contacts { get; set; }
    }

    public class Contact
    {
        [ValidateNever]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
        
        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }

        [ValidateNever]
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        
        [ValidateNever]
        public User? User { get; set; }
    }
}