using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// A set of simple plugins
namespace IrcBot.Utils
{
    /// <summary>
    /// A simple plugin that responds to pings.
    /// </summary>
    public class Ping : IPlugin
    {
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            return message == ".ping" ? "pong" : null;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }

    /// <summary>
    /// A plugin that gets the date. It takes an optional parameter for timezone
    /// </summary>
    /// <remarks>
    /// The timezone format takes system timezone names - see TimeZoneInfo.GetSystemTimeZones. On Windows, it takes formal names, (ex: Central Europe Standard Time) and on Unices, it uses the standard tzinfo format. (ex: Europe/Sarajevo)
    /// </remarks>
    public class Date : IPlugin
    {
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            if (message == ".date")
            {
                return DateTime.UtcNow.ToString() + " (UTC)";
            }
            if (message.StartsWith(".date "))
            {
                try
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(message.Remove(0, ".date ".Length))).ToString();
                }
                catch (TimeZoneNotFoundException e)
                {
                    return e.Message;
                }
            }

            return null;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }

    /// <summary>
    /// A plugin that rolls a die. It takes an optional parameter for the sides.
    /// </summary>
    public class Dice : IPlugin
    {
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            if (message == ".dice")
            {
                return new Random().Next(1, 7).ToString();
            }
            if (message.StartsWith(".dice "))
            {
                return new Random().Next(1, Convert.ToInt32(message.Split(' ')[1])).ToString();
            }

            return null;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }

    /// <summary>
    /// A plugin that flips a coin.
    /// </summary>
    public class Flip : IPlugin
    {
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            if (message == ".flip" || message == ".coin")
            {
                return Convert.ToBoolean(new Random().Next(2)) ? "heads" : "tails";
            }
            return null;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }

    public class Whois : IPlugin
    {
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            if (message.StartsWith(".whois "))
            {
                client.RfcWhois(message.Split(' ')[1]);
                IrcUser u = client.GetIrcUser(message.Split(' ')[1]);
                return String.Format("{0}!{1}@{2}, Away: {3}, Oper: {4}, Realname: {5}", u.Nick, u.Ident, u.Host, u.IsAway, u.IsIrcOp, u.Realname);
            }
            return null;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }

    /// <summary>
    /// Adds a command to add stuff to a topic.
    /// </summary>
    public class AppendTopic : IPlugin
    {
        const string Seperator = " | ";

        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            if (message.StartsWith(".appendtopic ") & source.StartsWith("#"))
            {
                string topic = client.GetChannel(source).Topic;
                string addition = message.Split(' ')[1];
                if (String.IsNullOrWhiteSpace(topic))
                {
                    client.RfcTopic(source, addition);
                    return null;
                }
                client.RfcTopic(source, topic + Seperator + addition);
            }
            return null; // we have no need to talk
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }

    public class Dns : IPlugin
    {
        string IPlugin.InvokeWithMessage(string source, string message, ref IrcClient client)
        {
            string toSend = String.Empty;
            if (message.StartsWith(".dns "))
            {
                IPHostEntry ip = System.Net.Dns.GetHostEntry(message.Split(' ')[1]);
                toSend = String.Format("Hostname: {0}; Addresses: {1}", ip.HostName, ip.AddressList.PrintEnumerable());
            }
            return toSend;
        }

        string IPlugin.InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client)
        {
            return null; // Not implemented
        }
    }
}
