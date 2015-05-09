using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ChallengeHarnessInterfaces
{
    [ServiceContract]
    public interface IBotRunner
    {
        [OperationContract]
        void Init(int playerNumber, String workingPath, String executableFilename);
        [OperationContract]
        int GetPlayerNumber();
        [OperationContract]
        string GetPlayerName();
        [OperationContract]
        string GetMove(MatchRender rendered);
        [OperationContract]
        string GetLog();
    }
}
