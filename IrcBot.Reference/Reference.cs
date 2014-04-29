using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using HtmlAgilityPack;
using System.Net;

// Plugins to get information from reference sites.
namespace IrcBot.Reference
{
    /// <summary>
    /// Fetch the first paragraph of a Wikipedia article.
    /// </summary>
    public class Wikipedia : IPlugin
    {
        const string RenderUrl = "http://en.wikipedia.org/w/index.php?action=render&title=";

        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            string toSend = null;
            // TODO: match wikipedia links
            if (message.StartsWith(".w"))
            {
                string url = RenderUrl + Uri.EscapeUriString(message.Split(new char[] { ' ' }, 2)[1]);
                string page;
                using (WebClient wc = new WebClient())
                {
                    page = wc.DownloadString(url);
                }
                HtmlDocument hd = new HtmlDocument();
                hd.LoadHtml(page);
                toSend = hd.DocumentNode.SelectSingleNode("/p").InnerText;
            }
            return toSend;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }
}
