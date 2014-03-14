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
    /// <remarks>You will have to implement them all, but nothing says you don't have to do anything useful with the ones you don't need.</remarks>
    public interface IPlugin
    {
        /// <summary>
        /// Activates the plugin with a message to parse.
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
        string InvokeWithMessage(string source, string message, ref IrcClient client);

        /// <summary>
        /// Activates the plugin with a channel user changing event.
        /// </summary>
        /// <param name="channel">If applicable, the channel the event was raised from.</param>
        /// <param name="user">The user that joined or left.</param>
        /// <param name="kicker">If applicable, the operator that kicked the user.</param>
        /// <param name="message">If applicable, the quit, part, or kick message.</param>
        /// <param name="type">The type of event that was raised.</param>
        /// <param name="client">A reference to the IRC client, for invoke commands with.</param>
        /// <returns>A message to send back to the channel. If there's nothing worth sending, you can return null to send nothing.</returns>
        /// <remarks>
        /// When using the IrcClient reference, don't invoke SendMessage to the source - the bot will do this to the source with what you return.
        /// 
        /// Plugins retain no state other than the IrcClient's when invoked.
        /// </remarks>
        string InvokeWithChannelUserChange(string channel, string user, string kicker, string message, ChannelUserChange type, ref IrcClient client);
    }

    /// <summary>
    /// The type of event when a channel's user is added or moved.
    /// </summary>
    public enum ChannelUserChange
    {
        Quit, Part, Join, Kick
    }
}
