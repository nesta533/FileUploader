using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploader
{
    internal abstract class Uploader
    {
        public abstract long UploadFile(string file);
    }
}
