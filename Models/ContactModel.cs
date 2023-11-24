using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APPMVC.NET.Models.Contacts 
{
    public class ContactModel 
    {
        [Key]
        public int ID { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [Display(Name = "Họ Tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Phải là địa chỉ email")]
        [Display(Name = "Địa chỉ email")]
        public string Email { get; set; }
        public DateTime DateSend { get; set; }

        [Display(Name = "Nội dung")]
        public string Message { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Phải là số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }
}