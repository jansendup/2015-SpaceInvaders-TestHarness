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
        [Option('p', "port", DefaultValue = "8081",
            HelpText = "Port to listen on")]
        public string Port { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
