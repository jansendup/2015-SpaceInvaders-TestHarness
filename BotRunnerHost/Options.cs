using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace BotRunnerHost
{
    class Options
    {
        [Option('x', "port", DefaultValue = 8081,
            HelpText = "Port to listen on")]
        public int Port { get; set; }

        [Option('s', "server", DefaultValue = null,
            HelpText = "Server where to publish bots")]
        public string Server { get; set; }

        [Option('u', "username", DefaultValue = "",
            HelpText = "Your username (email)")]
        public string Username { get; set; }

        [Option('p', "password", DefaultValue = "",
            HelpText = "Your password")]
        public string Password { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
