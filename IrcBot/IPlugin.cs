using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;

namespace IrcBot
{
    /// <summary>
    /// Represents a plugin implementation.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Activates the plugin.
        /// </summary>
        /// <param name="source">The channel or user that sent the message.</param>
        /// <param name="message">The message to parse.</param>
        /// <param name="client">A reference to the IRC client, for invoking commands with.</param>
        /// <returns>A message to send back to the source. If there's nothing worth sending, you can return null to send nothing.</returns>
        /// <remarks>
        /// When using the IrcClient reference, don't invoke SendMessage to the source - the bot will do this to the source with what you return.
        /// 
        /// Plugins retain no state other than the IrcClient's when invoked.
        /// 
        /// Conventions states that '.' should be the command prefix, if your plugin needs one.
        /// </remarks>
        /// <example>
        /// // This returns "Hello $source, I am $nickname" on every reply.
        /// public class Test : IPlugin {
        ///     string IPlugin.Invoke(string source, string message, ref IrcClient client) {
        ///         return "Hello " + source + " I am " + client.Nickname;
        ///     }
        /// }
        /// </example>
        string Invoke(string source, string message, ref IrcClient client);
    }
}
