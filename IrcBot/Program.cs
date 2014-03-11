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
	// HACK: Visual Studio cruft creates multiple builds of the plugin, resulting in multiple loads
        static List<string> pluginsByName = new List<string>();

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
                            // ok, i guess so - did we load it already?
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
                    Debug.Write(e.ToString(), "PluginLoading");
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.Write(e.ToString(), "PluginLoading");
                }
                catch (NullReferenceException e)
                {
                    Debug.Write(e.ToString(), "PluginLoading");
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
}
