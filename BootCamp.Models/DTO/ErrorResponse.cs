using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class ErrorResponse
    {
        public ErrorResponse()
        {
            this.Errors = new List<string>();
        }
        public ErrorResponse(string error)
        {
            this.Errors = new List<string>();
            this.AddError(error);
        }
        public ErrorResponse(List<string> errors)
        {
            this.Errors = new List<string>();
            this.AddErrors(errors);
        }
        public void AddError(string error)
        {
            if(Errors == null)
            {
                this.Errors = new List<string>();
            }
            Errors.Add(error);
        }
        public void AddErrors(List<string> errors)
        {
            if (Errors == null)
            {
                this.Errors = new List<string>();
            }
            this.Errors.AddRange(errors);
        }
        public List<string> Errors { get; set; }
    }
}
