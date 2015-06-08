using ChallengeHarness.Loggers;
using ChallengeHarness.Properties;
using ChallengeHarnessInterfaces;
using System;
using System.ServiceModel;
using System.Collections.Generic;

namespace ChallengeHarness.Runners
{
    public class MatchRunner
    {
        private readonly List<ILogger> _loggers = new List<ILogger>(3);
        private readonly MatchLogger _matchLogger = new MatchLogger();
        private readonly ReplayLogger _replayLogger;
        private readonly IBotRunner[] _players;

        public MatchRunner(IMatch match, string playerOneFolder, string playerTwoFolder, IRenderer renderer, bool consoleLoggingDisabled, bool consoleLoggingMustScroll, string replayFolder)
        {
            Match = match;
            Renderer = renderer;

            _players = new IBotRunner[2];
            if (playerOneFolder.Contains("@"))
            {
                string[] temp = playerOneFolder.Split('@');
                _players[0] = new RemoteBotRunner(1, temp[0], temp[1]);
            }
            else
            {
                _players[0] = new BotRunner(1, playerOneFolder);
            }
            if (playerTwoFolder.Contains("@"))
            {
                string[] temp = playerTwoFolder.Split('@');
                _players[1] = new RemoteBotRunner(2, temp[0], temp[1]);
            }
            else
            {
                _players[1] = new BotRunner(2, playerTwoFolder);
            }

            match.SetPlayerName(1, _players[0].GetPlayerName());
            match.SetPlayerName(2, _players[1].GetPlayerName());

            _replayLogger = new ReplayLogger(replayFolder);

            SetupLogging(consoleLoggingDisabled, consoleLoggingMustScroll);
        }

        protected void SetupLogging(bool consoleLoggingDisabled, bool consoleLoggingMustScroll)
        {
            _loggers.Add(_matchLogger);
            _loggers.Add(_replayLogger);

            if (consoleLoggingDisabled)
            {
                return;
            }

            var mapHeight = Renderer.Render(Match).Map.Split('\n').Length + 1 + 2 + 2; // +1 title line, +2 spacing lines and +2 move lines
            if ((consoleLoggingMustScroll) || (IsConsoleTooSmallForNormalLogging(mapHeight)))
            {
                _loggers.Add(new ConsoleScrollingLogger());
            }
            else
            {
                _loggers.Add(new ConsoleLogger());
            }
        }

        public IMatch Match { get; private set; }
        public IRenderer Renderer { get; private set; }

        public void Run()
        {
            do
            {
                var renderP1 = Renderer.Render(Match);
                var renderP2 = Renderer.Render(Match.GetFlippedCopyOfMatch());

                LogAll(renderP1);

                GetMove(_players[0], renderP1);
                GetMove(_players[1], renderP2);

                Match.Update();
            } while (!Match.GameIsOver());

            LogAll(Renderer.Render(Match));
            LogAll(Renderer.RenderSummary(Match));

            CopyLogs();
            _players[0].Destroy();
            _players[1].Destroy();
        }

        private void GetMove(IBotRunner player, MatchRender rendered)
        {
            var move = player.GetMove(rendered);
            Match.SetPlayerMove(player.GetPlayerNumber(), move);
        }

        private void LogAll(MatchRender renderP1)
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Log(renderP1);
            }
        }

        private void LogAll(MatchSummary summary)
        {
            foreach (ILogger logger in _loggers) {
                logger.Log(summary);
            }
        }

        private void CopyLogs()
        {
            _matchLogger.Close();
            _replayLogger.CopyMatchLog(_matchLogger.FileName);
            _replayLogger.WriteBotLog(_players[0].GetLog(), 1);
            _replayLogger.WriteBotLog(_players[1].GetLog(), 2);
        }

		private bool IsConsoleTooSmallForNormalLogging (int mapHeight)
		{
			return Console.WindowHeight < mapHeight;
		}
    }
}