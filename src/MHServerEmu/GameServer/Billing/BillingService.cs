﻿using Gazillion;
using MHServerEmu.Common.Config;
using MHServerEmu.Common.Logging;
using MHServerEmu.GameServer.Billing.Catalogs;
using MHServerEmu.GameServer.Entities;
using MHServerEmu.GameServer.GameData;
using MHServerEmu.GameServer.Properties;
using MHServerEmu.Networking;
using System.Text.Json;

namespace MHServerEmu.GameServer.Billing
{
    public class BillingService : IGameMessageHandler
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private readonly GameServerManager _gameServerManager;
        private readonly Catalog _catalog;

        public BillingService(GameServerManager gameServerManager)
        {
            _gameServerManager = gameServerManager;
            _catalog = JsonSerializer.Deserialize<Catalog>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Catalog.json")));

            // Apply a patch to the catalog if it's enabled and there's one
            if (ConfigManager.Billing.ApplyCatalogPatch)
            {
                string patchPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "CatalogPatch.json");
                if (File.Exists(patchPath))
                {
                    CatalogEntry[] catalogPatch = JsonSerializer.Deserialize<CatalogEntry[]>(File.ReadAllText(patchPath));
                    _catalog.ApplyPatch(catalogPatch);
                }
            }

            // Override store urls if enabled
            if (ConfigManager.Billing.OverrideStoreUrls)
            {
                _catalog.Urls[0].StoreHomePageUrl = ConfigManager.Billing.StoreHomePageUrl;
                _catalog.Urls[0].StoreBannerPageUrls[0].Url = ConfigManager.Billing.StoreHomeBannerPageUrl;
                _catalog.Urls[0].StoreBannerPageUrls[1].Url = ConfigManager.Billing.StoreHeroesBannerPageUrl;
                _catalog.Urls[0].StoreBannerPageUrls[2].Url = ConfigManager.Billing.StoreCostumesBannerPageUrl;
                _catalog.Urls[0].StoreBannerPageUrls[3].Url = ConfigManager.Billing.StoreBoostsBannerPageUrl;
                _catalog.Urls[0].StoreBannerPageUrls[4].Url = ConfigManager.Billing.StoreChestsBannerPageUrl;
                _catalog.Urls[0].StoreBannerPageUrls[5].Url = ConfigManager.Billing.StoreSpecialsBannerPageUrl;
                _catalog.Urls[0].StoreRealMoneyUrl = ConfigManager.Billing.StoreRealMoneyUrl;
            }

            Logger.Info($"Initialized store catalog with {_catalog.Entries.Length} entries");
        }

        public void Handle(FrontendClient client, ushort muxId, GameMessage message)
        {
            switch ((ClientToGameServerMessage)message.Id)
            {
                case ClientToGameServerMessage.NetMessageGetCatalog:
                    Logger.Info($"Received NetMessageGetCatalog");
                    client.SendMessage(muxId, new(_catalog.ToNetMessageCatalogItems(false)));
                    break;

                case ClientToGameServerMessage.NetMessageGetCurrencyBalance:
                    Logger.Info($"Received NetMessageGetCurrencyBalance");

                    client.SendMessage(muxId, new(NetMessageGetCurrencyBalanceResponse.CreateBuilder()
                        .SetCurrencyBalance(ConfigManager.Billing.CurrencyBalance)
                        .Build()));

                    break;

                case ClientToGameServerMessage.NetMessageBuyItemFromCatalog:
                    Logger.Info($"Received NetMessageBuyItemFromCatalog");
                    var buyItemMessage = NetMessageBuyItemFromCatalog.ParseFrom(message.Payload);
                    Logger.Trace(buyItemMessage.ToString());

                    // HACK: change costume when a player "buys" a costume
                    CatalogEntry entry = _catalog.GetEntry(buyItemMessage.SkuId);
                    if (entry != null && entry.GuidItems.Length > 0)
                    {
                        string prototypePath = GameDatabase.GetPrototypePath(entry.GuidItems[0].ItemPrototypeRuntimeIdForClient);
                        if (prototypePath.Contains("Entity/Items/Costumes/Prototypes/"))
                        {
                            // Create a new CostumeCurrent property for the purchased costume
                            Property property = new(PropertyEnum.CostumeCurrent, entry.GuidItems[0].ItemPrototypeRuntimeIdForClient);

                            // Get replication id for the client avatar
                            ulong replicationId = (ulong)Enum.Parse(typeof(HardcodedAvatarReplicationId), Enum.GetName(typeof(HardcodedAvatarEntity), client.Session.Account.PlayerData.Avatar));

                            // Update account data if needed
                            if (ConfigManager.Frontend.BypassAuth == false) client.Session.Account.PlayerData.CostumeOverride = entry.GuidItems[0].ItemPrototypeRuntimeIdForClient;

                            // Send NetMessageSetProperty message
                            client.SendMessage(1, new(property.ToNetMessageSetProperty(replicationId)));
                        }
                    }

                    client.SendMessage(muxId, new(NetMessageBuyItemFromCatalogResponse.CreateBuilder()
                        .SetDidSucceed(true)
                        .SetCurrentCurrencyBalance(ConfigManager.Billing.CurrencyBalance)
                        .SetErrorcode(BuyItemResultErrorCodes.BUY_RESULT_ERROR_SUCCESS)
                        .SetSkuId(buyItemMessage.SkuId)
                        .Build()));

                    break;

                default:
                    Logger.Warn($"Received unhandled message {(ClientToGameServerMessage)message.Id} (id {message.Id})");
                    break;
            }
        }

        public void Handle(FrontendClient client, ushort muxId, GameMessage[] messages)
        {
            foreach (GameMessage message in messages) Handle(client, muxId, message);
        }
    }
}
