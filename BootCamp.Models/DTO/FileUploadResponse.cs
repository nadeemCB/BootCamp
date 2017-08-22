using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class FileUploadResponse
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public long FileLength { get; set; }
    }
}
