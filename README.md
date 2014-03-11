# C# modularized IRC bot

A simple IrcBot. Requires SmartIrc4net, and HtmlAgilityPack (will by installed via NuGet) optionally. SmartIrc4net *may* require log4net to be linked into the core as well - if it's not needed, you can safely remove it.

## Design

Modules implement the `IPlugin` interface.

At first, it loads the files meeting `IrcBot.*.dll` match in the `PluginPath` setting, then loading the classes implementing `IPlugin`. On receiving a message, it will enumerate all plugins and invoke them. A plugin will only return non-null/`String.Empty` if it has something to say when parsing the message.

The method instance to use is:

    string IPlugin.Invoke(string source, string message, ref Meebey.SmartIrc4net.IrcClient client)

`source` indicates the channel or user (in case of private message) that the message originated from, `message` indicates the message, and `client` is a reference to the bot's instance of IrcClient, which can be used for commands. The return value is what to send back to the source.

Best practices when making your plugin is to use "." as a prefix for commands, and not to use `client.SendMessage`, as it will send to the source..

### Building a module

To build one, you can add a project to the solution or make a simple file with these parameters to the compiler manually:

    mcs -r:Meebey.SmartIrc4net -r:IrcBot.exe -target:library IrcBot.Plugin.cs

Whatever the case, make sure that SmartIrc4net and the executable itself are referenced.

## Modules

Modules MUST be prefixed with `IrcBot.*`.

* **IrcBot.Title**: Gets the titles of webpages linked.
* **IrcBot.Utils**: Implements a set of very simplistic plugins.
