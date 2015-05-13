using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using Newtonsoft.Json;

using CommandLine;
using System.ServiceModel.Description;
using System.Net;
using System.IO;

namespace BotRunnerHost
{
    class API
    {
        public class Bot
        {
            public string Author = "";
            public string Email = "";
            public string NickName = "";
        }

        public class BotUpdate
        {
            public string folder;
            public string author;
            public string email;
            public string nickName;

            public BotUpdate(Bot bot, string folder)
            {
                this.folder = folder;
                author = bot.Author;
                email = bot.Email;
                nickName = bot.NickName;
            }
        }

        public class Update
        {
            public string token;
            public int port;
            public BotUpdate[] bots;

            public Update(string token, int port, BotUpdate[] bots)
            {
                this.token = token;
                this.port = port;
                this.bots = bots;
            }
        }

        public class AuthResponse
        {
            public bool success = false;
            public string message = "";
            public string token = "";
        }

        public class Response
        {
            public bool success = false;
            public string message = "";
        }

        private string _url;
        private string _username;
        private string _password;
        private string _token;
        private int _port;
        private bool _shouldStop;

        private List<BotUpdate> _preUpdate;

        public API(string url, int port, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;
            _token = null;
            _port = port;
            _shouldStop = false;
            _preUpdate = null;
        }

        public API(string url, int port, string token)
        {
            _url = url;
            _username = null;
            _password = null;
            _token = token;
            _port = port;
            _shouldStop = false;
            _preUpdate = null;
        }

        public void Run()
        {
            if (_token == null)
            {
                if (!Authenticate()) {
                    Console.WriteLine("Failed to authenticate");
                    return;
                }
            }
            int counter = 0;
            while (!_shouldStop)
            {
                if (counter-- <= 0)
                {
                    UpdateBots();
                    counter = 60;
                }
                Thread.Sleep(1000);
            }
            ShutDown();
        }

        private string Send(string json, string path)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_url + "/api/" + path);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }

        private bool Authenticate()
        {
            string json = "{\"email\":\"" + _username + "\"," +
                              "\"password\":\"" + _password + "\"}";
            AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(Send(json, "authenticate"));
            if (!response.success)
            {
                Console.WriteLine(response.message);
            }
            else
            {
                _token = response.token;
            }
            return response.success;
        }

        private void UpdateBots()
        {
            List<BotUpdate> bots = new List<BotUpdate>();
            string currentDir = ".";
            string[] subDirs;
            try
            {
                subDirs = Directory.GetDirectories(currentDir);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            foreach (string subDir in subDirs)
            {
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(subDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Equals("bot.json"))
                    {
                        string fullPath = Path.GetFullPath(file).TrimEnd(Path.DirectorySeparatorChar);
                        string[] temp = fullPath.Split(Path.DirectorySeparatorChar);
                        string botname = temp[temp.Length - 2];
                        Bot bot = JsonConvert.DeserializeObject<Bot>(File.ReadAllText(file));
                        bots.Add(new BotUpdate(bot, botname));
                    }
                }
            }

            if (_preUpdate != null && _preUpdate.Count == bots.Count)
            {
                bool equal = true;
                foreach (BotUpdate bu1 in _preUpdate)
                {
                    bool found = false;
                    foreach (BotUpdate bu2 in bots)
                    {
                        if (bu1.author.Equals(bu2.author) && bu1.email.Equals(bu2.email) && bu1.folder.Equals(bu2.folder) && bu1.nickName.Equals(bu2.nickName))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal)
                {
                    Send("{\"token\":\"" + _token + "\", \"port\":" + _port + "}", "heartbeat");
                    return;
                }
            }

            Update update = new Update(_token, _port, bots.ToArray());
            string json = JsonConvert.SerializeObject(update);
            Response response = JsonConvert.DeserializeObject<Response>(Send(json, "register"));

            if (!response.success)
            {
                Console.WriteLine(response.message);
            }
            else
            {
                _preUpdate = bots;
            }

        }


        private void ShutDown()
        {
            Send("{\"token\":\"" + _token + "\", \"port\":" + _port + "}", "deregister");
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                return;
            }

            API api = null;
            Thread t = null;
            if (options.Server != null)
            {
                if (!options.Username.Equals("")) {
                    api = new API(options.Server, options.Port, options.Username, options.Password);
                    t = new Thread(new ThreadStart(api.Run));
                    t.Start();
                }
                else if (File.Exists("token.txt"))
                {
                    string token = File.ReadAllText("token.txt");
                    api = new API(options.Server, options.Port, token);
                    t = new Thread(new ThreadStart(api.Run));
                    t.Start();
                }
            }

            using (ServiceHost host = new ServiceHost(typeof(ChallengeHarness.Runners.BotRunner), new Uri("net.tcp://localhost:" + options.Port + "/")))
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
                host.AddServiceEndpoint(
                    typeof(ChallengeHarnessInterfaces.IBotRunner),
                    binding, "BotRunner");
                host.Open();
                Console.WriteLine("Host started @ " + DateTime.Now.ToString());
                Console.WriteLine("Press any key to stop");
                Console.ReadLine();
                if (api != null)
                {
                    api.RequestStop();
                    t.Join();
                }
            }
        }
    }
}
