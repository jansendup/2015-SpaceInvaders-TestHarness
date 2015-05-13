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
        public RemoteBotRunner(int playerNumber, string workingPath, string address)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxReceivedMessageSize = 512 * 1024;
            binding.MaxBufferSize = 512 * 1024;
            binding.MaxBufferPoolSize = 512 * 1024;
            binding.ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas();
            binding.ReaderQuotas.MaxArrayLength = 512 * 1024;
            binding.ReaderQuotas.MaxBytesPerRead = 512 * 1024;
            binding.ReaderQuotas.MaxDepth = 512 * 1024;
            binding.ReaderQuotas.MaxNameTableCharCount = 512 * 1024;
            binding.ReaderQuotas.MaxStringContentLength = 512 * 1024;
            NetTcpSecurity security = new NetTcpSecurity();
            security.Mode = SecurityMode.None;
            binding.Security = security;
            _client = null;
            _client = new Remote.BotRunnerClient(binding, new EndpointAddress("net.tcp://" + address + "/BotRunner"));
            if (!Init(playerNumber, workingPath))
            {
                throw new ApplicationException("Failed to initialize BotRunner");
            }
        }

        public void Destroy()
        {
            if (_client != null)
                _client.Destroy();
        }
        public bool Init(int playerNumber, string workingPath)
        {
            return _client.Init(playerNumber, workingPath);
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
