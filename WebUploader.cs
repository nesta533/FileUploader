using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace FileUploader
{
    class WebUploader : Uploader
    {

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string Ops { get; set; }

        private readonly WebClient web = new WebClient();

        public override long UploadFile(string file)
        {
            if (null == UserName && null == Password)
            {
                web.Credentials = new NetworkCredential();
            }
            else
            {
                web.Credentials = new NetworkCredential(UserName,Password);
            }

            web.UploadFile(Url, "")

            return 0;
        }
    }
}
