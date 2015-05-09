﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChallengeHarness.Remote {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="Remote.IBotRunner")]
    public interface IBotRunner {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/Init", ReplyAction="http://tempuri.org/IBotRunner/InitResponse")]
        void Init(int playerNumber, string workingPath, string executableFilename);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/Init", ReplyAction="http://tempuri.org/IBotRunner/InitResponse")]
        System.Threading.Tasks.Task InitAsync(int playerNumber, string workingPath, string executableFilename);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetPlayerNumber", ReplyAction="http://tempuri.org/IBotRunner/GetPlayerNumberResponse")]
        int GetPlayerNumber();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetPlayerNumber", ReplyAction="http://tempuri.org/IBotRunner/GetPlayerNumberResponse")]
        System.Threading.Tasks.Task<int> GetPlayerNumberAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetPlayerName", ReplyAction="http://tempuri.org/IBotRunner/GetPlayerNameResponse")]
        string GetPlayerName();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetPlayerName", ReplyAction="http://tempuri.org/IBotRunner/GetPlayerNameResponse")]
        System.Threading.Tasks.Task<string> GetPlayerNameAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetMove", ReplyAction="http://tempuri.org/IBotRunner/GetMoveResponse")]
        string GetMove(ChallengeHarnessInterfaces.MatchRender rendered);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetMove", ReplyAction="http://tempuri.org/IBotRunner/GetMoveResponse")]
        System.Threading.Tasks.Task<string> GetMoveAsync(ChallengeHarnessInterfaces.MatchRender rendered);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetLog", ReplyAction="http://tempuri.org/IBotRunner/GetLogResponse")]
        string GetLog();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBotRunner/GetLog", ReplyAction="http://tempuri.org/IBotRunner/GetLogResponse")]
        System.Threading.Tasks.Task<string> GetLogAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IBotRunnerChannel : ChallengeHarness.Remote.IBotRunner, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class BotRunnerClient : System.ServiceModel.ClientBase<ChallengeHarness.Remote.IBotRunner>, ChallengeHarness.Remote.IBotRunner {
        
        public BotRunnerClient() {
        }
        
        public BotRunnerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public BotRunnerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public BotRunnerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public BotRunnerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void Init(int playerNumber, string workingPath, string executableFilename) {
            base.Channel.Init(playerNumber, workingPath, executableFilename);
        }
        
        public System.Threading.Tasks.Task InitAsync(int playerNumber, string workingPath, string executableFilename) {
            return base.Channel.InitAsync(playerNumber, workingPath, executableFilename);
        }
        
        public int GetPlayerNumber() {
            return base.Channel.GetPlayerNumber();
        }
        
        public System.Threading.Tasks.Task<int> GetPlayerNumberAsync() {
            return base.Channel.GetPlayerNumberAsync();
        }
        
        public string GetPlayerName() {
            return base.Channel.GetPlayerName();
        }
        
        public System.Threading.Tasks.Task<string> GetPlayerNameAsync() {
            return base.Channel.GetPlayerNameAsync();
        }
        
        public string GetMove(ChallengeHarnessInterfaces.MatchRender rendered) {
            return base.Channel.GetMove(rendered);
        }
        
        public System.Threading.Tasks.Task<string> GetMoveAsync(ChallengeHarnessInterfaces.MatchRender rendered) {
            return base.Channel.GetMoveAsync(rendered);
        }
        
        public string GetLog() {
            return base.Channel.GetLog();
        }
        
        public System.Threading.Tasks.Task<string> GetLogAsync() {
            return base.Channel.GetLogAsync();
        }
    }
}
