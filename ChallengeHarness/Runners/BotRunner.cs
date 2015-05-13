using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ChallengeHarness.Properties;
using ChallengeHarnessInterfaces;
using Newtonsoft.Json;

namespace ChallengeHarness.Runners
{
    public class BotRunner : IBotRunner
    {
        private Stopwatch _botTimer;
        private MemoryStream _inMemoryLog;
        private StreamWriter _inMemoryLogWriter;
        private string _mapFilename;
        private string _moveFilename;
        private string _stateFilename;
        private string _workingPath;
        private string _botLogFilename;
        private string _lockFilename;
        private string _processName;

        private int _playerNumber;
        private string _playerName;

        private bool _have_lock;

        public BotRunner()
        {
            _have_lock = false;
        }

        public BotRunner(int playerNumber, String workingPath)
        {
            if (!Init(playerNumber, workingPath))
            {
                throw new ApplicationException("Failed to initialize BotRunner");
            }
        }

        ~BotRunner()
        {
            Destroy();
        }

        public void Destroy()
        {
            if (_have_lock && File.Exists(_lockFilename))
            {
                File.Delete(_lockFilename);
                _have_lock = false;
            }
        }

        public bool Init(int playerNumber, String workingPath)
        {
            try {
                string executableFilename = Environment.OSVersion.Platform == PlatformID.Unix ? Settings.Default.BotRunFilenameLinux : Settings.Default.BotRunFilename;
                _inMemoryLog = new MemoryStream();
                _inMemoryLogWriter = new StreamWriter(_inMemoryLog);
                _botTimer = new Stopwatch();

                _workingPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + workingPath;
                _mapFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder, Settings.Default.MapFilename);
                _stateFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder, Settings.Default.StateFilename);
                _moveFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder, Settings.Default.MoveFileName);
                _processName = Path.Combine(_workingPath, executableFilename);
                _lockFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder, Settings.Default.BotRunLockFilename);
                _botLogFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder,
                    Settings.Default.BotLogFilename);

                if (File.Exists(_lockFilename))
                {
                    if ( DateTime.Now.Subtract(File.GetCreationTime(_lockFilename)).Minutes >= 20 )
                    {
                        File.Delete(_lockFilename);
                    }
                    else
                    {
                        _have_lock = false;
                        return false;
                    }
                }

                _playerNumber = playerNumber;
                _playerName = LoadBotName();

                CreateOutputDirectoryIfNotExists();
                ClearAllOutputFiles();
                File.Create(_lockFilename).Close();
                _have_lock = true;
                return true;
            }
            catch (Exception) { return false; }
            
        }

        public int GetPlayerNumber() { return _playerNumber; }
        public string GetPlayerName() { return _playerName; }

        private string LoadBotName()
        {
            BotMeta metaData;
            try
            {
                string textData;
                using (
                    var file =
                        new StreamReader(_workingPath + Path.DirectorySeparatorChar +
                                         Settings.Default.BotMetaDataFilename))
                {
                    textData = file.ReadToEnd();
                }

                metaData = JsonConvert.DeserializeObject<BotMeta>(textData);
            }
            catch
            {
                return "Player " + _playerNumber;
            }

            return metaData.NickName;
        }

        public string GetMove(MatchRender rendered)
        {
            OutputFile(_mapFilename, rendered.Map);
            OutputFile(_stateFilename, rendered.State);

            _botTimer.Reset();
            _botTimer.Start();

            var process = CreateProcess();
            AddEventHandlersToProcess(process);
            StartProcess(process);
            var result = HandleProcessResponse(process);

            AppendLogs();
            ClearRoundFiles();

            return result;
        }

        private void CreateOutputDirectoryIfNotExists()
        {
            var outputFolder = Path.Combine(_workingPath, Settings.Default.BotOutputFolder);
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
        }

        public void OutputAppendLog(string logEntry)
        {
            _inMemoryLogWriter.WriteLine(logEntry);
        }

        private void AppendLogs()
        {
            _inMemoryLogWriter.Flush();
            Debug.WriteLine("Saving player " + _playerNumber + " bot log to: " + _botLogFilename);
            using (var file = File.Open(_botLogFilename, FileMode.Append))
            {
                _inMemoryLog.WriteTo(file);
            }

            _inMemoryLog.SetLength(0);
        }

        private void OutputFile(string filename, string value)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            using (var file = new StreamWriter(filename))
            {
                file.WriteLine(value);
            }
        }

        private void ClearAllOutputFiles()
        {
            File.Delete(_mapFilename);
            File.Delete(_stateFilename);
            File.Delete(_moveFilename);
            File.Delete(_botLogFilename);
        }

        private void ClearRoundFiles()
        {
            File.Delete(_mapFilename);
            File.Delete(_stateFilename);
            File.Delete(_moveFilename);
        }

        private Process CreateProcess()
        {
            if (!File.Exists(_processName))
            {
                throw new FileNotFoundException("Bot process file '" + _processName + "' not found.");
            }

            var arguments = " \"" + Settings.Default.BotOutputFolder + "\"";
            var processName = _processName;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                arguments = _processName + " " + arguments;
                processName = "/bin/bash";
            }

            return new Process
            {
                StartInfo =
                {
                    WorkingDirectory = _workingPath,
                    FileName = processName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
        }

        private void AddEventHandlersToProcess(Process p)
        {
            DataReceivedEventHandler h = (sender, args) =>
            {
                if (!String.IsNullOrEmpty(args.Data))
                {
                    _inMemoryLogWriter.WriteLine(args.Data);
                }
            };
            p.OutputDataReceived += h;
            p.ErrorDataReceived += h;
        }

        private void StartProcess(Process p)
        {
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            var didExit = p.WaitForExit(Settings.Default.MoveTimeoutSeconds * 1000);
            _botTimer.Stop();

            if (!didExit)
            {
                if (!p.HasExited)
                    p.Kill();
                OutputAppendLog(String.Format("[GAME]\tBot {0} timed out after {1} ms.", _playerName,
                    _botTimer.ElapsedMilliseconds));
                OutputAppendLog(String.Format("[GAME]\tKilled process {0}.", _processName));
            }
            else
            {
                OutputAppendLog(String.Format("[GAME]\tBot {0} finished in {1} ms.", _playerName,
                    _botTimer.ElapsedMilliseconds));
            }

            if ((didExit) && (p.ExitCode != 0))
            {
                OutputAppendLog(String.Format("[GAME]\tProcess exited with non-zero code {0} from player {1}.",
                    p.ExitCode, _playerName));
            }
        }

        private string HandleProcessResponse(Process p)
        {
            if (!File.Exists(_moveFilename))
            {
                OutputAppendLog("[GAME]\tNo output file from player " + _playerName);
                return null;
            }

            var fileLines = File.ReadAllLines(_moveFilename);
            return fileLines.Length > 0 ? fileLines[0] : null;
        }

        public string GetLog()
        {
            return File.ReadAllText(_botLogFilename);
        }
    }
}
