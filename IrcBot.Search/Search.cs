using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using System.Xml;

namespace IrcBot.Search
{
    /// <summary>
    /// Searches DuckDuckGo for a zero-click answer.
    /// </summary>
    public class Search : IPlugin
    {
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            string toSend = null;
            if (message.StartsWith(".ddg"))
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(GetDDGApiUrl(message.Split(new char[] { ' ' }, 2)[1]));
                if (xd.SelectSingleNode("/DuckDuckGoResponse/Answer") != null)
                {
                    toSend = xd.SelectSingleNode("/DuckDuckGoResponse/Answer").InnerText;
                }
                // dear diary, can I have my ?. operator soon?
                if (xd.SelectSingleNode("/DuckDuckGoResponse/Abstract") != null)
                {
                    if (!String.IsNullOrWhiteSpace(xd.SelectSingleNode("/DuckDuckGoResponse/Abstract").InnerText))
                    {
                        toSend = String.Format("{0} - {1}", xd.SelectSingleNode("/DuckDuckGoResponse/Abstract").InnerText, xd.SelectSingleNode("/DuckDuckGoResponse/AbstractURL").InnerText);
                    }
                }
            }
            return toSend;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }

        static string GetDDGApiUrl(string query)
        {
            return String.Format("https://api.duckduckgo.com/?q={0}&format=xml&t=IrcBot.Search&no_redirect=1&no_html=1&skip_disambig=1", Uri.EscapeUriString(query));
        }
    }
}
