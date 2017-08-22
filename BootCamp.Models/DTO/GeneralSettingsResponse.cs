using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class GeneralSettingsResponse:SuccessResponse
    {
        public string AppVersion { get; set; }
        public bool Critical { get; set; }
        public string S3 { get; set; }
    }
}
