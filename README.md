## house bot

# Weapons

### Handgun — Basic sidearm for close encounters. (compact, stackable if you choose)
### Shotgun — Close-range heavy hitter. "One shot, one chunk."
### SniperRifle — Long-range precision rifle for single-target elimination.
### AssaultRifle — Full-auto mid/long range rifle. Good all-rounder for suppressive fire.
### LMG (Light Machine Gun) — High-capacity weapon for sustained firing; spray-and-pray tool.
### Crossbow — Silent, single-shot ranged weapon; feels like a medieval assassin tool.
### RayGun — Exotic / wonder-weapon. Powerful, usually not purchasable from normal vendors.
### RayGun Mark II — Upgraded exotic energy weapon; higher performance than RayGun.
### Wunderwaffe DG-2 — Extremely powerful wonder-weapon (very rare / special).
### Thunder Gun — Another wonder-weapon with unique (stunning/flying) effects.

# Medical

### Medkit — Restores health / heals wounds.
### Painkillers — Reduces damage taken temporarily or eases pain effects.
### Morphine — Strong painkiller; temporarily restores health and reduces pain effects.
### Experimental Serum — Unpredictable effects; powerful but risky.
### Fentanyl — Extremely potent opioid (very high potency / high risk).
### Vicodin — Prescription opioid variant (powerful medical effect in-game).

# Drugs / Stimulants

### Caffeine Pill — Small alertness/boost for short durations.
### Adrenaline Shot — Short burst of speed and damage.
### Cocaine — Short, intense energy boost; risky side effects.
### LSD — Hallucinogen; long duration with perception alteration.
### Methamphetamine — Strong stimulant; powerful but with heavy side effects/addiction risk.
### Blue Methamphetamine — High-grade/potent version of meth (rare / legendary).
### Heroin — Powerful opioid with strong effects and addiction risk.

# Food

### Apple — Small health restore / snack.
### Bread — Modest health restore.
### Salad — Health + small energy buff.
### Burger — Large health & energy restore.
### Sushi — High-value food with health + morale boost.
### Steak — Substantial health restore.
### Legendary Feast — Massive restoration + temporary stat boosts (legendary item).

# Tools

### Lockpick — Used for bypassing locks, burglary tools.
### Toolkit — Repair / craft toolset.
### Backpack — Increases carry/storage; utility item.

# Knives / Melee

### Katana — Large melee blade; non-stackable.
### Dagger — Small blade; quick strikes.
### BowieKnife — Heavy combat knife (serrated).
### Karambit — Curved tactical knife.
### Switchblade — Concealable folding knife.
```
House
├─ House.Attributes
│  ├─ IsConfigExisting.cs
│  ├─ IsGuildOwner.cs
│  ├─ IsOwnerAttribute.cs
│  ├─ IsPlayerAttribute.cs
│  ├─ IsStaffAttribute.cs
│  └─ IsStaffOrOwner.cs
├─ House.Converters
│  └─ Converters.cs
├─ House.Core
│  ├─ Bot.cs
│  ├─ BotEventHandlers.cs
│  ├─ Config.cs
│  ├─ HelpFormatter.cs
│  ├─ HouseBaseEvent.cs
│  └─ HouseEventDispatcher.cs
├─ House.Events
│  ├─ CommandErroredEvent.cs
│  ├─ MessageCreatedEvent.cs
│  ├─ MessageDeletedEvent.cs
│  ├─ MessageReactionAddedEvent.cs
│  └─ ReadyEvent.cs
├─ House.Extensions
│  ├─ DiscordClientExtensions.cs
│  ├─ DiscordImageHelper.cs
│  ├─ RandomExtensions.cs
│  ├─ RarityExtensions.cs
│  └─ StringExtensions.cs
├─ House.Modules
│  ├─ BackupModule.cs
│  ├─ CoomerModule.cs
│  ├─ EconomyGamesModule.cs
│  ├─ EconomyModule.cs
│  ├─ EconomyVendorModule.cs
│  ├─ FunModule.cs
│  ├─ GeneralModule.cs
│  ├─ HelpModule.cs
│  ├─ OwnerModule.cs
│  ├─ StaffModule.cs
│  ├─ TestModule.cs
│  └─ VoiceModule.cs
├─ House.Services
│  ├─ Database
│  │  ├─ BackupGuild.cs
│  │  ├─ BlacklistedUser.cs
│  │  ├─ DatabaseEntity.cs
│  │  ├─ DatabaseGuild.cs
│  │  ├─ DatabaseUser.cs
│  │  ├─ Exceptions.cs
│  │  ├─ Position.cs
│  │  ├─ ProtectionLevel.cs
│  │  ├─ Repository.cs
│  │  ├─ SnipedMessage.cs
│  │  ├─ StaffUser.cs
│  │  ├─ StarboardEntry.cs
│  │  └─ WhitelistedUser.cs
│  ├─ Economy
│  │  ├─ Exceptions.cs
│  │  ├─ General
│  │  │  ├─ AmmoType.cs
│  │  │  ├─ GlobalItemPool.cs
│  │  │  ├─ HouseEconomyItem.cs
│  │  │  ├─ HouseInventoryConstants.cs
│  │  │  ├─ HouseItemType.cs
│  │  │  ├─ LootTable.cs
│  │  │  ├─ PurchaseResult.cs
│  │  │  └─ Rarity.cs
│  │  ├─ HouseEconomyDatabase.cs
│  │  ├─ HouseEconomyUser.cs
│  │  ├─ Items
│  │  │  ├─ Consumables.cs
│  │  │  ├─ Gun.cs
│  │  │  ├─ Knife.cs
│  │  │  └─ Tools.cs
│  │  ├─ Managers.cs
│  │  ├─ Market
│  │  │  ├─ HouseStockMarket.cs
│  │  │  └─ MarketAutoUpdater.cs
│  │  └─ Vendors
│  │     ├─ HouseEconomyVendor.cs
│  │     ├─ VendorAutoRestocker.cs
│  │     ├─ VendorPresets.cs
│  │     └─ VendorType.cs
│  ├─ Fuzzy
│  │  ├─ HouseFuzzyMatchingService.cs
│  │  └─ HouseFuzzyResult.cs
│  ├─ Gooning
│  │  ├─ Exceptions
│  │  │  ├─ CoomerHTTPExceptions.cs
│  │  │  └─ CoomerServiceExceptions.cs
│  │  ├─ GooningService.cs
│  │  └─ HTTP
│  │     ├─ CoomerCache.cs
│  │     ├─ CoomerClient.cs
│  │     ├─ CoomerCreator.cs
│  │     ├─ CoomerEncryption.cs
│  │     ├─ CoomerFile.cs
│  │     ├─ CoomerPost.cs
│  │     ├─ CoomerService.cs
│  │     ├─ Endpoints.cs
│  │     └─ UserCoomerData.cs
│  ├─ Gooning.zip
│  └─ Protection
│     ├─ AntiNukeService.cs
│     ├─ BackupService.cs
│     ├─ ServiceThresholds.cs
│     ├─ SuspectManager.cs
│     └─ SuspectMember.cs
├─ House.Utils
│  ├─ CoomerUtils.cs
│  ├─ EconomyUtils.cs
│  └─ EmbedUtils.cs
├─ House.csproj
├─ House.sln
├─ Program.cs
└─ README.md

```