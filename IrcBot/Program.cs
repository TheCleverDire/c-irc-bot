using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using IrcBot.Properties;
using System.Threading;

namespace IrcBot
{
    class Program
    {
        static IrcClient i = new IrcClient();
        // Plugin information
        static List<Type> plugins = new List<Type>();

        static void Main(string[] args)
        {
            // load plugins
            foreach (string f in Directory.EnumerateFiles(Settings.Default.PluginPath, "IrcBot.*.dll", SearchOption.AllDirectories))
            {
                Debug.WriteLine("Attempting to load " + f, "PluginLoading");
                RecursiveAssemblyLoader r = new RecursiveAssemblyLoader(); // to load plugin deps
                Assembly file = r.GetAssembly(Path.GetFullPath(f));
                try
                {
                    // get types
                    foreach (Type t in file.GetTypes())
                    {
                        // see if they're a valid plugin
                        if (t.GetInterface("IPlugin").IsEquivalentTo(typeof(IPlugin)))
                        {
                            // ok, i guess so - did we load it already?
                            if (plugins.Contains(t)) continue;
                            // add plugin
                            plugins.Add(t);
                        }
                    }
                    Debug.WriteLine("Loaded " + file, "PluginLoading");
                }
                catch (InvalidCastException e)
                {
                    Debug.WriteLine(e.ToString(), "PluginLoading");
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.WriteLine(e.ToString(), "PluginLoading");
                }
                catch (NullReferenceException e)
                {
                    Debug.WriteLine(e.ToString(), "PluginLoading");
                }
            }
            // assign events
            i.OnChannelMessage += i_OnChannelMessage;
            i.OnQueryMessage += i_OnQueryMessage;
            i.OnJoin += i_OnJoin;
            i.OnQuit += i_OnQuit;
            i.OnKick += i_OnKick;
            i.OnPart += i_OnPart;
            // init
            i.ActiveChannelSyncing = true;
            i.UseSsl = Settings.Default.SSL;
            i.Connect(Settings.Default.Servers.Cast<string>().ToArray(), Settings.Default.Port);
            i.Login(Settings.Default.Nickname, Settings.Default.RealName);
            i.RfcJoin(Settings.Default.Channels.Cast<string>().ToArray());
            i.Listen();
        }

        static void i_OnPart(object sender, PartEventArgs e)
        {
            InvokePluginWithChannelUserChange(e.Channel, e.Who, null, e.PartMessage, ChannelUserChange.Part);
        }

        static void i_OnKick(object sender, KickEventArgs e)
        {
            // SmartIrc4net documentation is NOT clear on the Who/Whom mixup
            InvokePluginWithChannelUserChange(e.Channel, e.Whom, e.Who, e.KickReason, ChannelUserChange.Kick);
        }

        static void i_OnQuit(object sender, QuitEventArgs e)
        {
            InvokePluginWithChannelUserChange(null, e.Who, null, e.QuitMessage, ChannelUserChange.Quit);
        }

        static void i_OnJoin(object sender, JoinEventArgs e)
        {
            InvokePluginWithChannelUserChange(e.Channel, e.Who, null, null, ChannelUserChange.Join);
        }

        static void i_OnQueryMessage(object sender, IrcEventArgs e)
        {
            InvokePluginWithMessage(e.Data.Nick, e.Data.Message);
        }

        static void i_OnChannelMessage(object sender, IrcEventArgs e)
        {
            InvokePluginWithMessage(e.Data.Channel, e.Data.Message);
        }

        /// <summary>
        /// Invokes a plugin with a message.
        /// </summary>
        /// <param name="source">The channel or user that sent the message.</param>
        /// <param name="message">The message for the plugins to handle.</param>
        static void InvokePluginWithMessage(string source, string message)
        {
            // This doesn't matter anyways if it succeeds or not
            try
            {
                foreach (Type p in plugins)
                {
                    Thread t = new Thread(() =>
                    {
                        // Why not spawn instances when we loaded them? Sometimes state likes to stick or something.
                        string to_send = ((IPlugin)Activator.CreateInstance(p)).InvokeWithMessage(source, message, ref i);
                        if (!message.IsNullOrWhitespace())
                        {
                            i.SendMessage(SendType.Message, source, to_send);
                        }
                    });
                    t.IsBackground = true;
                    t.Name = p.Name;
                    t.Start();
                    if (!t.Join(5000))
                    {
                        t.Abort();
                        Debug.WriteLine(String.Format("The plugin ({0}) took too long to respond for InvokePluginWithMessage.", p.Name), "PluginLoader");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        static void InvokePluginWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type)
        {
            // This doesn't matter anyways if it succeeds or not
            try
            {
                foreach (Type p in plugins)
                {
                    Thread t = new Thread(() =>
                    {
                        // Why not spawn instances when we loaded them? Sometimes state likes to stick or something.
                        string to_send = ((IPlugin)Activator.CreateInstance(p)).InvokeWithChannelUserChange(channel, user, kicker, message, type, ref i);
                        if (!message.IsNullOrWhitespace())
                        {
                            i.SendMessage(SendType.Message, channel, to_send);
                        }
                    });
                    t.IsBackground = true;
                    t.Name = p.Name;
                    t.Start();
                    if (!t.Join(5000))
                    {
                        t.Abort();
                        Debug.WriteLine(String.Format("The plugin ({0}) took too long to respond for InvokePluginWithChannelUserChange.", p.Name), "PluginLoader");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Recursively loads an assembly and its references.
    /// </summary>
    /// <see cref="http://stackoverflow.com/a/3059289"/>
    public class RecursiveAssemblyLoader : MarshalByRefObject
    {
        public Assembly GetAssembly(string path)
        {
            return Assembly.LoadFrom(path);
        }
    }

    /// <summary>
    /// Assorted utility functions, useful for plugins.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Prints an enumerable as a string representation of all the contained items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l">The enumerable to print.</param>
        /// <param name="sep">The string seperator to use.</param>
        /// <returns>A string representation of the enumerable.</returns>
        public static string PrintEnumerable<T>(this IEnumerable<T> l, string sep)
        {
            string printed = String.Empty;

            if (l.Count() != 0)
            {
                foreach (var i in l.Take(l.Count()))
                {
                    printed += (i.ToString() + sep);
                }
            }

            return printed;
        }

        /// <summary>
        /// An extension method version of String.IsNullOrEmpty.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>If the string is null or empty.</returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return String.IsNullOrEmpty(s);
        }

        /// <summary>
        /// An extension method version of String.IsNullOrWhiteSpace.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>If the string is null, empty, or whitespace.</returns>
        public static bool IsNullOrWhitespace(this string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }


    }
}
