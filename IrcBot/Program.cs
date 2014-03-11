// HACK: Everything about this.
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

namespace IrcBot
{
    class Program
    {
        static IrcClient i = new IrcClient();
        // Plugin information
        static List<IPlugin> plugins = new List<IPlugin>();
        static List<string> pluginsByName = new List<string>(); // plugin names, as checking typenames would be expensive

        static void Main(string[] args)
        {
            // load plugins
            foreach (string f in Directory.EnumerateFiles(Settings.Default.PluginPath, "IrcBot.*.dll", SearchOption.AllDirectories))
            {
                Debug.Write("Attempting to load " + f, "PluginLoading");
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
                            // ok, i guess so - did we load it already? (this is because of general VS cruft it leaves)
                            if (pluginsByName.Contains(t.Name)) continue;
                            // add plugin
                            pluginsByName.Add(t.Name);
                            plugins.Add((IPlugin)Activator.CreateInstance(t));
                        }
                    }
                    Debug.WriteLine("Loaded " + file, "PluginLoading");
                }
                catch (InvalidCastException e)
                {
                    //Debug.Write(e.ToString(), "PluginLoading");
                }
                catch (ReflectionTypeLoadException e)
                {
                    //Debug.Write(e.ToString(), "PluginLoading");
                }
                catch (NullReferenceException e)
                {
                    //Debug.Write(e.ToString(), "PluginLoading");
                }
            }
            // assign events
            i.OnChannelMessage += i_OnChannelMessage;
            i.OnQueryMessage += i_OnQueryMessage;
            // init
            i.ActiveChannelSyncing = true;
            i.UseSsl = Settings.Default.SSL;
            i.Connect(Settings.Default.Servers.Cast<string>().ToArray(), Settings.Default.Port);
            i.Login(Settings.Default.Nickname, Settings.Default.RealName);
            i.RfcJoin(Settings.Default.Channels.Cast<string>().ToArray());
            i.Listen();
        }

        static void i_OnQueryMessage(object sender, IrcEventArgs e)
        {
            // This doesn't matter anyways if it succeeds or not
            try
            {
                foreach (IPlugin p in plugins)
                {
                    string message = p.Invoke(e.Data.Nick, e.Data.Message, ref i);
                    if (!String.IsNullOrEmpty(message))
                    {
                        i.SendMessage(SendType.Message, e.Data.Nick, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        static void i_OnChannelMessage(object sender, IrcEventArgs e)
        {
            // This doesn't matter anyways if it succeeds or not
            try
            {
                foreach (IPlugin p in plugins)
                {
                    string message = p.Invoke(e.Data.From, e.Data.Message, ref i);
                    if (!String.IsNullOrEmpty(message))
                    {
                        i.SendMessage(SendType.Message, e.Data.Channel, message);
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
    /// Recursively loads an assembly and it's references.
    /// </summary>
    /// <see cref="http://stackoverflow.com/a/3059289"/>
    public class RecursiveAssemblyLoader : MarshalByRefObject
    {
        public Assembly GetAssembly(string path)
        {
            return Assembly.LoadFrom(path);
        }
    }

    /** 
     * in retrospect this was a bad idea, marked here for my remembering not to do it
     * example usage: return FunctionHandler.Handler("date", source, message, ref client, (string s, ref IrcClient c) => { return DateTime.UtcNow.ToString() + " (UTC)"; }, (string s, string m, ref IrcClient c) => { try { return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(m)).ToString(); } catch (TimeZoneNotFoundException e) { return e.Message; } });
     * yeah...
    public static class FunctionHandler
    {
        /// <summary>
        /// Represents a function for <see cref="Handler"/> to use.
        /// </summary>
        /// <param name="source">The channel or user that the message originated from.</param>
        /// <param name="message">The message to parse.</param>
        /// <param name="client">A reference to the IrcClient that can be used to invoke further tasks.</param>
        /// <returns>A message to send, null if nothing to send.</returns>
        public delegate string HandlerFuncWithParams(string source, string message, ref IrcClient client);
        /// <summary>
        /// Represents a function for <see cref="Handler"/> to use.
        /// </summary>
        /// <param name="source">The channel or user that the message originated from.</param>
        /// <param name="client">A reference to the IrcClient that can be used to invoke further tasks.</param>
        /// <returns>A message to send, null if nothing to send.</returns>
        public delegate string HandlerFunc(string source, ref IrcClient client);

        /// <summary>
        /// Parses a message and executes a function depending on if it has parameters or not, and returns null if there was no function anyways.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <param name="noParams"></param>
        /// <param name="withParams"></param>
        /// <returns></returns>
        public static string Handler(string name, string source, string message, ref IrcClient client, HandlerFunc noParams, HandlerFuncWithParams withParams)
        {
            if (("." + name) == message)
            {
                return noParams(source, ref client);
            }
            if (message.StartsWith("." + name))
            {
                return withParams(source, message.Remove(0, ("." + name).Length), ref client);
            }
            return null;
        }

        public static string Handler(string name, string source, string message, ref IrcClient client, HandlerFunc noParams)
        {
            if (("." + name) == message)
            {
                return noParams(source, ref client);
            }
            return null;
        }
    } **/
}
