using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

using CommandLine;
using System.ServiceModel.Description;

namespace BotRunnerHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                return;
            }
            using (ServiceHost host = new ServiceHost(typeof(ChallengeHarness.Runners.BotRunner)))
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.MaxReceivedMessageSize = 512 * 1024;
                NetTcpSecurity security = new NetTcpSecurity();
                security.Mode = SecurityMode.None;
                binding.Security = security;
                host.AddServiceEndpoint(
                    typeof(ChallengeHarnessInterfaces.IBotRunner),
                    binding,
                    "net.tcp://0.0.0.0:" + options.Port + "/BotRunner");
                host.Open();
                Console.WriteLine("Host started @ " + DateTime.Now.ToString());
                Console.ReadLine();
            }
        }
    }
}
