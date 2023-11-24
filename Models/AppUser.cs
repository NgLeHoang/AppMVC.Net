using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace APPMVC.NET.Models {
    public class AppUser : IdentityUser {
        [Column(TypeName = "nvarchar")]
        [StringLength(255)]
        public string? HomeAddress { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
    }

}