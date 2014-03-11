using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        string IPlugin.Invoke(string source, string message, ref IrcClient client)
        {
            return message == ".ping" ? "pong" : null;
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
        string IPlugin.Invoke(string source, string message, ref IrcClient client)
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
    }

    /// <summary>
    /// A plugin that rolls a die. It takes an optional parameter for the sides.
    /// </summary>
    public class Dice : IPlugin
    {
        string IPlugin.Invoke(string source, string message, ref IrcClient client)
        {
            if (message == ".dice")
            {
                return new Random().Next(1, 7).ToString();
            }
            if (message.StartsWith(".dice "))
            {
                return new Random().Next(1, Convert.ToInt32(message.Remove(0, ".dice ".Length))).ToString();
            }

            return null;
        }
    }

    /// <summary>
    /// A plugin that flips a coin.
    /// </summary>
    public class Flip : IPlugin
    {
        string IPlugin.Invoke(string source, string message, ref IrcClient client)
        {
            if (message == ".flip" || message == ".coin")
            {
                return Convert.ToBoolean(new Random().Next(2)) ? "heads" : "tails";
            }
            return null;
        }
    }

    public class Whois : IPlugin
    {
        string IPlugin.Invoke(string source, string message, ref IrcClient client)
        {
            if (message.StartsWith(".whois "))
            {
                //return client.GetIrcUser(message.Remove(0, ".whois ".Length)).ToString();
                client.RfcWhois(message.Remove(0, ".whois ".Length));

                IrcUser u = client.GetIrcUser(message.Remove(0, ".whois ".Length));
                return u.ToString();
            }
            return null;
        }
    }

    /// <summary>
    /// Adds a command to add stuff to a topic.
    /// </summary>
    public class AppendTopic : IPlugin
    {
        string IPlugin.Invoke(string source, string message, ref IrcClient client)
        {
            if (message.StartsWith(".appendtopic"))
            {
                Debug.WriteLine(client.GetChannel(source).Topic);
                return client.GetChannel(source).Topic;
            }
            return null; // we have no need to talk
        }
    }
}
