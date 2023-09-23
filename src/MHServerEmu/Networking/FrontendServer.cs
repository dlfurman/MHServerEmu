﻿using MHServerEmu.Common.Config;
using MHServerEmu.Common.Logging;
using MHServerEmu.GameServer;
using MHServerEmu.GameServer.Frontend;
using MHServerEmu.Networking.Base;

namespace MHServerEmu.Networking
{
    public class FrontendServer : Server
    {
        private new static readonly Logger Logger = LogManager.CreateLogger();  // Hide the Server.Logger so that this logger can show the actual server as log source.

        private readonly GameServerManager _gameServerManager;

        public FrontendService FrontendService { get => _gameServerManager.FrontendService; }

        public FrontendServer()
        {
            _gameServerManager = new();

            OnConnect += FrontendServer_OnConnect;
            OnDisconnect += FrontendService.OnClientDisconnect;
            DataReceived += FrontendServer_DataReceived;
            DataSent += (sender, e) => { };
        }

        public override void Run()
        {
            if (Listen(ConfigManager.Frontend.BindIP, int.Parse(ConfigManager.Frontend.Port)) == false) return;
            Logger.Info($"FrontendServer is listening on {ConfigManager.Frontend.BindIP}:{ConfigManager.Frontend.Port}...");
        }

        private void FrontendServer_OnConnect(object sender, ConnectionEventArgs e)
        {
            Logger.Info($"Client connected from {e.Connection}");
            e.Connection.Client = new FrontendClient(e.Connection, _gameServerManager);
        }

        private void FrontendServer_DataReceived(object sender, ConnectionDataEventArgs e)
        {
            ((FrontendClient)e.Connection.Client).Parse(e);
        }
    }
}
