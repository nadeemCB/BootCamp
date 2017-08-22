using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class PaymentRequest
    {
        [Required(ErrorMessage ="Payment is required")]
        [Range(0.1, 999,ErrorMessage ="Payment amount should be between 0.1 to 999")]
        public decimal PaymentAmount { get; set; }
    }
}
