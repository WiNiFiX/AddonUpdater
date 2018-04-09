using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddonUpdater
{
    public class Addon
    {
        private readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public string Name;
        public string Author;
        public string CurrentVersion;
        public string LatestVersion;
        public string DownloadURL;
        public bool Update;
        public long LiveEpochDate;
        public DateTime LatestDate => epoch.AddSeconds(LiveEpochDate);
        public void Download()
        {
            // https://wow.curseforge.com/projects/bagnon?gameCategorySlug=addons&amp;projectID=1592
            // https://wow.curseforge.com/projects/bagnon/files/latest

            try
            {
                var URL = DownloadURL.Split('?')[0] + "/files/latest";
                var zipFileName = Path.Combine(Application.StartupPath, Name + "-" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip");

                using (var client = new WebClient())
                {
                    client.DownloadFile(URL, zipFileName);
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
