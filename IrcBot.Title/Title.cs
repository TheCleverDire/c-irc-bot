using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace IrcBot.Title
{
    public class Title : IPlugin
    {
        // THANKS GRUBER
        const string UrlMatchExpression = @"(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'" + "\"" + ".,<>?«»“”‘’]))";

        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            string toSend = String.Empty; // Make csc happy
            // catch urls
            MatchCollection matches = Regex.Matches(message, UrlMatchExpression);
            foreach (Match m in matches)
            {
                if (!(m.Value.StartsWith("http://") || m.Value.StartsWith("https://"))) continue; // boo unprefix

                try
                {
                    // Check if that's even an HTML file
                    WebRequest wr = WebRequest.Create(m.Value);
                    wr.Method = "HEAD";
                    string type = String.Empty;
                    using (WebResponse wrr = wr.GetResponse())
                    {
                        Debug.WriteLine("Found type " + wrr.ContentType, "TitlePlugin");
                        type = wrr.ContentType;
                    }
                    // TODO: Support a whole bunch of wacky shit. img2aa anyone?
                    // Go through the types. We StartWith because of encoding info.
                    if (type.StartsWith("text/html"))
                    {
                        // We could support the other wacky shit like XML
                        toSend = GetHTMLGist(m.Value);
                    }
                }
                catch (WebException e)
                {
                    Debug.WriteLine(e.ToString(), "TitlePlugin");
                    return null; // we failed
                }
                catch (UriFormatException e)
                {
                    Debug.WriteLine(e.ToString(), "TitlePlugin");
                    return null; // we failed
                }
            }
            return !toSend.IsNullOrWhitespace() ? toSend : null;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }

        /// <summary>
        /// Gets the title and any other interesting doodads of an HTML document.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>The title, maybe other stuff..</returns>
        static string GetHTMLGist(string url)
        {
            HtmlDocument hd = new HtmlDocument();
            using (WebClient wc = new WebClient())
            {
                hd.LoadHtml(wc.DownloadString(url));
            }
            try
            {
                string title = hd.DocumentNode.SelectSingleNode("//title").InnerText;
                Debug.WriteLine("Title is " + title, "TitlePlugin");
                return title;
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e.ToString(), "TitlePlugin");
                return null;
            }
        }
    }
}
