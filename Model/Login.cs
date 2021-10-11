using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MessagingProducerAPI.Model
{
    public class Login
    {
        [Key]
        public int userID { set; get; }

        [Required]
        [StringLength(100)]
        //[RegularExpression(@"\b[A-Za-z0-9._%-]+@(\reqres\live\.wcs\.ac\.uk\.com)\b")]
        public string email { set; get; } = null;

        [Required]
        [StringLength(100)]
        public string password { set; get; } = null;

        public string task { set; get; } = null;
    }
}
