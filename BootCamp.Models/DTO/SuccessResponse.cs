using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class SuccessResponse
    {
        public SuccessResponse()
        {
        }
        public SuccessResponse(string message)
        {
            this.Message = message;
        }        
        public string Message { get; set; }
    }
}

