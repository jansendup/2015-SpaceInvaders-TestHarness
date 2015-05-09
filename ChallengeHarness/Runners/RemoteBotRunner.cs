using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChallengeHarnessInterfaces;
using System.ServiceModel;

namespace ChallengeHarness.Runners
{
    public class RemoteBotRunner : IBotRunner
    {
        private Remote.BotRunnerClient _client;
        public RemoteBotRunner(int playerNumber, string workingPath, string executableFilename, string address)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxReceivedMessageSize = 512*1024;
            _client = new Remote.BotRunnerClient(binding, new EndpointAddress("net.tcp://" + address + "/BotRunner"));
            Init(playerNumber, workingPath, executableFilename);
        }
        public void Init(int playerNumber, string workingPath, string executableFilename)
        {
            _client.Init(playerNumber, workingPath, executableFilename);
        }

        public int GetPlayerNumber()
        {
            return _client.GetPlayerNumber();
        }

        public string GetPlayerName()
        {
            return _client.GetPlayerName();
        }

        public string GetMove(MatchRender rendered)
        {
            return _client.GetMove(rendered);
        }

        public string GetLog()
        {
            return _client.GetLog();
        }
    }
}
