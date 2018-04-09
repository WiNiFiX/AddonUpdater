using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace AddonUpdater
{
    public static class Addons
    {
        private static string WoWFolder => @"C:\Games\World of Warcraft";
        private static string AddonFolder => $@"{WoWFolder}\Interface\AddOns";
        
        private static List<Addon> currentAddons;
        
        private static int PathDepth(string path)
        {
            return path.Split('\\').Length;
        }

        public static void UpdateChecked()
        {
            foreach (Addon current in currentAddons.Where(a => a.Update == true))
            {
                current.Download();
            }
        }
        
        public static void MarkForUpdate(string addonName, bool update)
        {
            var addon = currentAddons.FirstOrDefault(a => a.Name == addonName);
            addon.Update = update;
        }

        public static void PopulateMyAddons(DataGridView dataGrid)
        {
            currentAddons = new List<Addon>();
            DirectoryInfo di = new DirectoryInfo(AddonFolder);

            foreach(FileInfo fi in di.GetFiles("*.toc", SearchOption.AllDirectories))
            {
                var depth = PathDepth(fi.FullName);
                var addonDepth = PathDepth(AddonFolder);

                if (depth == addonDepth + 2) // We dont want to look more than 1 folder depth from root, which is 2 in depth of '\' count
                {
                    var diAddon = Directory.GetParent(fi.FullName);
                    if (diAddon.GetFiles().Length == 1) 
                        continue;
                    
                    var a = ParseToc(fi);
                    if (a.Name != null)
                    {
                        var u = UpdateAddonInfo(a);
                        try
                        {
                            u.Update = a.CurrentVersion != u.LatestVersion;
                            dataGrid.Rows.Add(u.Name, u.Author, a.CurrentVersion, u.LatestVersion ?? u.LatestDate.ToString("yyyy/MM/dd"), u.Update);
                            currentAddons.Add(u);
                        }
                        catch
                        {
                            //dataGrid.Rows.Add(a.Name, a.Author, a.CurrentVersion, "Not found", false);
                        }
                    }                    
                }
                dataGrid.Sort(dataGrid.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
            }            
        }

        private static void Search(string AddonName)
        {
            WebClient wc = new WebClient();
            var html = wc.DownloadString("https://wow.curseforge.com/search/get-results?providerIdent=projects&search=Bagnon");
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
                        
            var nodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'results')]"); // This contains all the table rows and colums as part of the "results" class

            foreach (var node in nodes.Where(x => x.ChildNodes.Count >= 7))
            {
                Addon a = new Addon();
                var tempName = node.ChildNodes[3].InnerText.Replace("\r", "").Replace("\n", "").Trim();
                tempName = tempName.Replace("  ", "^");
                a.Name = tempName.Split('^')[0];
                a.Author = node.ChildNodes[5].InnerText.Replace("\r", "").Replace("\n", "").Trim();                                
                a.LiveEpochDate = long.Parse(node.ChildNodes[7].ChildNodes[1].Attributes["data-epoch"].Value);
                //currentAddonsSearched.Add(a);
            }
        }

        private static Addon UpdateAddonInfo(Addon inputAddon)
        {
            if (inputAddon.Name == "Lib: LibDFramework-1.0")
                inputAddon.Name = "LibDFramework";

            if (inputAddon.Name == "Deadly Boss Mods")
                inputAddon.Name = "Deadly Boss Mods (DBM)";

            var addonNameToMatch = inputAddon.Name;
            Addon ret = new Addon();

            WebClient wc = new WebClient();
            var url = "https://wow.curseforge.com/search/get-results?providerIdent=projects&search=" + HttpUtility.UrlEncode(inputAddon.Name);
            var html = wc.DownloadString(url);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'results')]"); // This contains all the table rows and colums as part of the "results" class

            if (nodes != null)
            {
                foreach (var node in nodes.Where(x => x.ChildNodes.Count >= 7))
                {                    
                    var tempName = node.ChildNodes[3].InnerText.Replace("\r", "").Replace("\n", "").Trim();
                    tempName = tempName.Replace("  ", "^");
                    ret.Name = tempName.Split('^')[0];
                    ret.Author = node.ChildNodes[5].InnerText.Replace("\r", "").Replace("\n", "").Trim();                                        
                    ret.DownloadURL = "https://wow.curseforge.com" + node.ChildNodes[3].ChildNodes[1].ChildNodes[1].OuterHtml.Replace("<a href=\"", "").Split('>')[0].Replace("\"", "");
                    // https://wow.curseforge.com/projects/bagnon?gameCategorySlug=addons&amp;projectID=1592
                    // https://wow.curseforge.com/projects/bagnon/files/latest

                    try
                    {
                        ret.LiveEpochDate = long.Parse(node.ChildNodes[7].ChildNodes[1].Attributes["data-epoch"].Value);
                    }
                    catch
                    {
                        ret.LiveEpochDate = 0;
                    }

                    if (ret.Name == addonNameToMatch)
                        return ret;
                }
            }
                        
            url = "https://www.wowace.com/search/get-results?providerIdent=projects&search=" + HttpUtility.UrlEncode(inputAddon.Name);
            html = wc.DownloadString(url);
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            nodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'results')]"); // This contains all the table rows and colums as part of the "results" class

            if (nodes == null)
            {
                return inputAddon;
            }

            foreach (var node in nodes.Where(x => x.ChildNodes.Count >= 7))
            {                
                var tempName = node.ChildNodes[3].InnerText.Replace("\r", "").Replace("\n", "").Trim();
                tempName = tempName.Replace("  ", "^");
                ret.Name = tempName.Split('^')[0];
                ret.Author = node.ChildNodes[5].InnerText.Replace("\r", "").Replace("\n", "").Trim();
                ret.DownloadURL = "https://www.wowace.com" + node.ChildNodes[3].ChildNodes[1].ChildNodes[1].OuterHtml.Replace("<a href=\"", "").Split('>')[0].Replace("\"", "");
                
                try
                {
                    ret.LiveEpochDate = long.Parse(node.ChildNodes[7].ChildNodes[1].Attributes["data-epoch"].Value);
                }
                catch
                {
                    ret.LiveEpochDate = 0;
                }

                if (ret.Name == addonNameToMatch)
                    return ret;
            }

            return null;
        }

        private static Addon ParseToc(FileInfo fi)
        {               
            //currentAddonsSearched = new List<Addon>();

            var tocContents = File.ReadAllText(fi.FullName);
            
            Addon currentAddon = new Addon();
            if (tocContents.Contains("## RequiredDeps: DBM-Core") || fi.FullName.EndsWith("DBM-StatusBarTimers.toc")) // Skip Broken DBM stuff - things without proper toc files
                return currentAddon;

            foreach (string fileLine in tocContents.Split('\n'))
            {
                var line = fileLine.Replace("\r", "");
                if (line.StartsWith("## Title:"))
                {
                    line = line.Replace("## Title:", "").Trim();
                    
                    if (line.Contains("|")) // Handle funny colors on addon titles that people like using eg: "cffffd200Deadly Boss Mods"
                    {
                        line = line.Split('|')[1].Substring(9);
                    }

                    currentAddon.Name = line;
                }

                if (line.StartsWith("## Author: "))
                {
                    currentAddon.Author = line.Replace("## Author: ", "");                    
                }

                // Versions are random and not always updated so we use a more reliable last modified date of the *.toc file
                //if (line.StartsWith("## Version: "))
                //    currentAddon.CurrentVersion = line.Replace("## Version: ", "");                                
            }

            if (currentAddon.CurrentVersion == null)
            {
                currentAddon.CurrentVersion = File.GetCreationTimeUtc(fi.FullName).ToString("yyyy/MM/dd");
            }
            
            return currentAddon;
        }
    }
}
