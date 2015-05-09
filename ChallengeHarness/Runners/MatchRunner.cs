using ChallengeHarness.Loggers;
using ChallengeHarness.Properties;
using ChallengeHarnessInterfaces;
using System;
using System.ServiceModel;

namespace ChallengeHarness.Runners
{
    public class MatchRunner
    {
        private readonly ConsoleLogger _consoleLogger;
        private readonly MatchLogger _logger;
        private readonly IBotRunner[] _players;
        private readonly ReplayLogger _replayLogger;

        public MatchRunner(IMatch match, string playerOneFolder, string playerTwoFolder, IRenderer renderer)
        {
            Match = match;
            Renderer = renderer;

            _logger = new MatchLogger();
            _consoleLogger = new ConsoleLogger();
            _replayLogger = new ReplayLogger();

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
        }

        private void GetMove(IBotRunner player, MatchRender rendered)
        {
            var move = player.GetMove(rendered);
            Match.SetPlayerMove(player.GetPlayerNumber(), move);
        }

        private void LogAll(MatchRender renderP1)
        {
            _logger.Log(renderP1);
            _replayLogger.Log(renderP1);
            _consoleLogger.Log(renderP1);
        }

        private void LogAll(MatchSummary summary)
        {
            _logger.Log(summary);
            _replayLogger.Log(summary);
            _consoleLogger.Log(summary);
        }

        private void CopyLogs()
        {
            _logger.Close();

            _replayLogger.CopyMatchLog(_logger.FileName);
            _replayLogger.WriteBotLog(_players[0].GetLog(), 1);
            _replayLogger.WriteBotLog(_players[1].GetLog(), 2);
        }
    }
}