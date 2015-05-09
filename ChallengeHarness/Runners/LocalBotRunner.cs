using System;
using System.Diagnostics;
using System.IO;
using ChallengeHarness.Properties;
using ChallengeHarnessInterfaces;
using Newtonsoft.Json;

namespace ChallengeHarness.Runners
{
    public class LocalBotRunner : BotRunner
    {
        private readonly string _mapFilename;
        private readonly string _moveFilename;
        private readonly string _stateFilename;
        private string _processName;

        public LocalBotRunner(int playerNumber, String workingPath, String executableFilename) : base(playerNumber, workingPath)
        {
            _mapFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder, Settings.Default.MapFilename);
            _stateFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder, Settings.Default.StateFilename);
            _moveFilename = Path.Combine(_workingPath, Settings.Default.BotOutputFolder, Settings.Default.MoveFileName);

            _processName = Path.Combine(_workingPath, executableFilename);
        }

        protected override void Init()
        {
            PlayerName = LoadBotName();
            ClearRoundFiles();
        }

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
                return "Player " + PlayerNumber;
            }

            return metaData.NickName;
        }

        protected override string ExecuteBot(MatchRender rendered)
        {
            _botTimer.Reset();
            _botTimer.Start();

            OutputFile(_mapFilename, rendered.Map);
            OutputFile(_stateFilename, rendered.State);

            var process = CreateProcess();
            AddEventHandlersToProcess(process);
            StartProcess(process);
            return HandleProcessResponse(process);
        }

        private Process CreateProcess()
		{
			if (!File.Exists (_processName)) {
				throw new FileNotFoundException ("Bot process file '" + _processName + "' not found.");
			}

			var arguments = " \"" + Settings.Default.BotOutputFolder + "\"";
			var processName = _processName;
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
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

            var didExit = p.WaitForExit(Settings.Default.MoveTimeoutSeconds*1000);
            _botTimer.Stop();

            if (!didExit)
            {
                OutputAppendLog(String.Format("[GAME]\tBot {0} timed out after {1} ms.", PlayerName,
                    _botTimer.ElapsedMilliseconds));
                OutputAppendLog(String.Format("[GAME]\tKilled process {0}.", _processName));
            }
            else
            {
                OutputAppendLog(String.Format("[GAME]\tBot {0} finished in {1} ms.", PlayerName,
                    _botTimer.ElapsedMilliseconds));
            }

            if (p.ExitCode != 0)
            {
                OutputAppendLog(String.Format("[GAME]\tProcess exited with non-zero code {0} from player {1}.",
                    p.ExitCode, PlayerName));
            }
        }

        private string HandleProcessResponse(Process p)
        {
            if (!File.Exists(_moveFilename))
            {
                OutputAppendLog("[GAME]\tNo output file from player " + PlayerName);
                return null;
            }

            var fileLines = File.ReadAllLines(_moveFilename);
            return fileLines.Length > 0 ? fileLines[0] : null;
        }

        protected override void ClearRoundFiles()
        {
            File.Delete(_mapFilename);
            File.Delete(_moveFilename);
            File.Delete(_stateFilename);
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

    }
}