# STS2 v0.113.1 relic compatibility review

Baseline: v0.110.1 (`db5d3552`). Installed game: v0.113.1 (`d09d48e5`).
Assembly MVID: `9d417b39-1d40-4fc7-b993-28acdc29e151`.

Scanned 317 relic types: 17 added, 0 removed, 81 changed existing types.
Fingerprint differences include compiler-generated state-machine renumbering; a changed fingerprint is a review signal, not proof of a gameplay change.

Supported release pairs are listed in the [README](../README.md#supported-versions). Mod `1.0.9` is verified for game `v0.113.1`; the `v0.110.1` comparison baseline does not imply backward DLL compatibility. Use Mod `1.0.8` for game `v0.107.1` / `v0.110.1` as documented by that release. Later game builds remain unverified.

## Added relics

BathRobes, BathTowel, BizarreLamp, Byrdine, ConductorsBaton, ExerciseMat, Fertilizer, MightyWings, ObsidianVice, Ocarina, SongBow, Storyteller, Thumbscrew, TormentousCollar, Twine, WhittlingKnife, Wuppy.

## Changed relics

AlchemicalCoffer, AmethystAubergine, ArchaicTooth, Astrolabe, BeautifulBracelet, BingBong, BookOfFiveRings, BookRepairKnife, BowlerHat, BrilliantScarf, BurningSticks, Byrdpip, CallingBell, DelicateFrond, DistinguishedCape, DragonFruit, DreamCatcher, DustyTome, FishingRod, ForgottenSoul, FragrantMushroom, FrozenEgg, FurCoat, Girya, GlassEye, GremlinHorn, JeweledMask, JossPaper, JuzuBracelet, Kaleidoscope, Kusarigama, LargeCapsule, LastingCandy, LeafyPoultice, LetterOpener, LordsParasol, LostCoffer, LuckyFysh, MassiveScroll, MealTicket, MembershipCard, MoltenEgg, NeowsBones, NewLeaf, OldCoin, Orrery, PaelsEye, PaelsLegion, PaelsWing, PandorasBox, ParryingShield, Permafrost, PhialHolster, Planisphere, PotionBelt, PowerCell, PrayerWheel, RainbowRing, Regalite, RingingTriangle, RoyalStamp, SandCastle, ScrollBoxes, SereTalon, Shovel, SilverCrucible, Tingsha, ToastyMittens, TouchOfOrobas, ToxicEgg, ToyBox, VelvetChoker, WarHammer, WarPaint, Whetstone, WhisperingEarring, WhiteBeastStatue, WhiteStar, WingCharm, WingedBoots, YummyCookie.

## Confirmed broken shared API targets

- Multi-target `CreatureCmd.Damage` gained `ModifyDamageHookType`: CharonsAshes, LostWisp, MiniatureCannon, MysticLighter, PenNib, StrikeDummy.
- Merchant purchase methods changed: FrozenEgg, MoltenEgg, ToxicEgg, LordsParasol, MembershipCard, TheCourier.
- `RingingTriangle.ShouldFlush` was removed; review the replacement effect from `AfterRoomEntered`.
- `CardSelectCmd.FromChooseABundleScreen` gained a boolean parameter (ScrollBoxes).

The review and adaptation results follow.

## Adaptation results

The implementation follows the existing independent per-relic Prefix/Postfix and ContinueWith pattern. The previous RelicEffectCapture file, AsyncLocal scopes, global effect probes and shared Task wrappers have been removed. There is no new runtime helper framework.

| Scope | Minimal adaptation |
| --- | --- |
| Core damage API | Six existing relic patch groups add the DamageTargetData overload used by AttackCommand.Execute, including ModifyDamageHookType. Existing counting routines remain. |
| PenNib, StrikeDummy | Remove the old single-target wrapper patch so the shared game damage core is counted only once. |
| MiniatureCannon, MysticLighter, StrikeDummy, PenNib | Respect the new additive/multiplicative hook flag so cap-only damage does not count a skipped relic modifier. |
| FrozenEgg, MoltenEgg, ToxicEgg | Rename the merchant upgrade target to MerchantCardEntry.OnTryPurchaseInternal; retain the original callbacks. |
| MembershipCard, TheCourier, LordsParasol | Rename the purchase target to MerchantEntry.OnTryPurchaseUnsynchronized. The new base method also handles removal, so the obsolete separate removal patches are deleted. Original state/callback logic is retained. LordsParasol checks the entry owner. |
| RingingTriangle | Move the existing ShouldFlush patch to RetainHandPower.ShouldFlush and resolve the owning RingingTriangle on the first turn. Keep the old whole-hand count and combat/turn deduplication; no new Retain or RunicPyramid exclusions. |
| Regalite | Keep the original implementation and add one UsedThisTurn guard. |
| BurningSticks | Keep the original used-flag activation counter. Bind the renamed third argument by index and correct the comment about when the flag changes. |
| BookRepairKnife | Change the eligible Doom deaths filter to enemies, matching the updated game implementation. |
| ScrollBoxes | Add the new selection boolean argument. Existing offered/selected card tracking is unchanged. |
| Added relics | Add one independent patch group and stats definition for each of the 17 new types, following existing relic implementations. |
| Remaining fingerprint changes | Reviewed RNG migrations, availability / async renumbering, Fool upgrade mappings, balance values and trigger reordering. Preserve existing patches where their targets and counting still match. |

## Added relic counters

| Relic | Counter |
| --- | --- |
| `BathRobes` | HP Healed |
| `BathTowel` | HP Healed |
| `BizarreLamp` | Cards Generated |
| `Byrdine` | God Byrds Played |
| `ConductorsBaton` | Damage Mitigated (before Wuppy redirection and block, including rounding and damage caps) |
| `ExerciseMat` | Stretch Gained |
| `Fertilizer` | Strength Gained |
| `MightyWings` | Strength Gained only, matching the other strength-granting relics |
| `ObsidianVice` | Damage Dealt |
| `Ocarina` | Wah / Cho / Ki Cards Generated, counted separately from successful generation results |
| `SongBow` | Aim Gained |
| `Storyteller` | Aim Gained |
| `Thumbscrew` | Extra Hits Added, counted from executed attack results rather than preview queries |
| `TormentousCollar` | Weak Applied |
| `Twine` | Block Gained |
| `WhittlingKnife` | Aim Gained |
| `Wuppy` | HP Blocked, counted from actual damage redirected to Wuppy (excluding initialization, healing and overkill) |

Healing uses before/after HP snapshots. Buff and block counters follow the existing per-relic trigger conditions and dynamic values; generated-card and damage counters use their operation results. These are independent relic patches, with no shared effect-capture layer. The Baton uses synchronous comparison calculations only inside actual damage commands; comparison flags never cross an await. Two existing modifier counters skip these comparison calculations to avoid double counting. MightyWings only records strength gained; its extra-damage calculations, per-combat strength state and three related hooks have been removed. Existing strength counts remain visible, while legacy extra-damage/state keys are hidden. Polluted pre-fix Thumbscrew and Wuppy keys are not reused for the corrected metrics.

## Verification

- Build against the installed v0.113.1 assembly: passed with zero warnings and errors.
- 449 declared/manual Harmony target checks: passed.
- Generated stats: 317 game relic types, 317 definitions; no missing types.
- .NET 9 runtime mounting: all 440 patch classes mounted with no failures; all six save/history hooks and dynamic lifecycle hooks mounted.
- Core damage routing: all six affected groups patch the DamageTargetData core exactly once.
- Cross-version saved-stat logic: 13 version/build cases passed using the production persistence source with an in-memory test store. Mismatched snapshots keep their original counters, text and provenance when loaded and re-saved; history uses archived display. This does not test running the current DLL on older game binaries.
- English/Simplified Chinese: 185 matching nonblank entries with valid placeholders/tags; all 317 formatters passed 3,804 live/history/zero/populated output checks under eng -> zhs -> eng, including fallback and version banners. Official game PCK terms checked; Stretch, God Byrd, Abundance and Wah/Cho/Ki Chinese labels corrected, and ScrollBoxes Box 1/2 headings translated. Both packaged language files match the checked sources.
- Manual-test feedback regression: 258 assertions passed with the production patch files linked into deterministic settlement fixtures. Cases cover repeated previews, Exterminate's fifth hit, early kills, Wuppy initialization/healing/overkill, asynchronous melody generation, damage caps and unrelated modifier counters. Baton mitigation includes damage saved for Wuppy and is counted before player block; Wuppy at 60/16/3/0 HP is covered. MightyWings records strength for eligible hits only; legacy Strength Gained: 4 is preserved while Extra Damage Dealt: 2 and the old per-combat state are hidden in live/history tooltips. These fixtures do not host Godot gameplay.
- Full Godot-hosted combat, merchant UI and save/load gameplay have not been exercised.

## Complete method fingerprint differences

These are exact v0.110.1-to-v0.113.1 fingerprint differences, including generated async methods. The old cached DLL used as supplementary source evidence has 295 of 300 relic fingerprints identical to the saved baseline; BeautifulBracelet, JeweledMask, PaelsEye, Regalite and ToastyMittens were separately reviewed against their current implementations.

| Relic | Method differences |
| --- | --- |
| `AlchemicalCoffer` | `body AlchemicalCoffer/<AfterObtained>d__7.MoveNext():System.Void` |
| `AmethystAubergine` | `removed AmethystAubergine.IsAllowed(Runs.IRunState):System.Boolean` |
| `ArchaicTooth` | `body ArchaicTooth.get_TranscendenceUpgrades():System.Collections.Generic.Dictionary`2<Models.ModelId,Models.CardModel>` |
| `Astrolabe` | `body Astrolabe/<AfterObtained>d__6.MoveNext():System.Void` |
| `BeautifulBracelet` | `body BeautifulBracelet.AfterObtained():System.Threading.Tasks.Task`<br>`added BeautifulBracelet/<>c__DisplayClass7_0..ctor():System.Void`<br>`added BeautifulBracelet/<>c__DisplayClass7_0.<AfterObtained>b__0(Models.CardModel):System.Boolean`<br>`removed BeautifulBracelet/<AfterObtained>d__7.MoveNext():System.Void`<br>`removed BeautifulBracelet/<AfterObtained>d__7.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `BingBong` | `type shape changed`<br>`body BingBong.AfterCardChangedPiles(Models.CardModel,Entities.Cards.PileType,Models.AbstractModel):System.Threading.Tasks.Task`<br>`added BingBong/<AfterCardChangedPiles>d__2.MoveNext():System.Void`<br>`added BingBong/<AfterCardChangedPiles>d__2.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed BingBong.get_CardsToSkip():System.Collections.Generic.HashSet`1<Models.CardModel>`<br>`removed BingBong/<AfterCardChangedPiles>d__5.MoveNext():System.Void`<br>`removed BingBong/<AfterCardChangedPiles>d__5.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `BookOfFiveRings` | `body BookOfFiveRings.AfterCardChangedPiles(Models.CardModel,Entities.Cards.PileType,Models.AbstractModel):System.Threading.Tasks.Task`<br>`body BookOfFiveRings.DoActivateVisuals():System.Threading.Tasks.Task`<br>`added BookOfFiveRings/<AfterCardChangedPiles>d__18.MoveNext():System.Void`<br>`added BookOfFiveRings/<AfterCardChangedPiles>d__18.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`added BookOfFiveRings/<DoActivateVisuals>d__19.MoveNext():System.Void`<br>`added BookOfFiveRings/<DoActivateVisuals>d__19.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed BookOfFiveRings.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed BookOfFiveRings/<AfterCardChangedPiles>d__19.MoveNext():System.Void`<br>`removed BookOfFiveRings/<AfterCardChangedPiles>d__19.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed BookOfFiveRings/<DoActivateVisuals>d__20.MoveNext():System.Void`<br>`removed BookOfFiveRings/<DoActivateVisuals>d__20.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `BookRepairKnife` | `body BookRepairKnife.AfterDiedToDoom(GameActions.Multiplayer.PlayerChoiceContext,System.Collections.Generic.IReadOnlyList`1<Entities.Creatures.Creature>):System.Threading.Tasks.Task`<br>`added BookRepairKnife/<>c.<AfterDiedToDoom>b__6_0(Entities.Creatures.Creature):System.Boolean`<br>`removed BookRepairKnife.<AfterDiedToDoom>b__6_0(Entities.Creatures.Creature):System.Boolean` |
| `BowlerHat` | `removed BowlerHat.IsAllowed(Runs.IRunState):System.Boolean` |
| `BrilliantScarf` | `body BrilliantScarf.get_CanonicalVars():System.Collections.Generic.IEnumerable`1<Localization.DynamicVars.DynamicVar>` |
| `BurningSticks` | `body BurningSticks/<AfterCardExhausted>d__9.MoveNext():System.Void` |
| `Byrdpip` | `type shape changed`<br>`added Byrdpip.AfterCardPlayed(GameActions.Multiplayer.PlayerChoiceContext,Entities.Cards.CardPlay):System.Threading.Tasks.Task`<br>`body Byrdpip.AfterObtained():System.Threading.Tasks.Task`<br>`body Byrdpip.BeforeCombatStart():System.Threading.Tasks.Task`<br>`added Byrdpip.get_CanonicalVars():System.Collections.Generic.IEnumerable`1<Localization.DynamicVars.DynamicVar>`<br>`added Byrdpip.get_DisplayAmount():System.Int32`<br>`added Byrdpip.get_ShowCounter():System.Boolean`<br>`added Byrdpip.get_SwoopsPlayed():System.Int32`<br>`added Byrdpip.set_SwoopsPlayed(System.Int32):System.Void`<br>`body Byrdpip.SummonPet():System.Threading.Tasks.Task`<br>`added Byrdpip.TryModifyRestSiteOptions(Entities.Players.Player,System.Collections.Generic.ICollection`1<Entities.RestSite.RestSiteOption>):System.Boolean`<br>`added Byrdpip.UpdateDisplay():System.Void`<br>`added Byrdpip/<>c.<AfterObtained>b__27_0(Models.CardModel):System.Boolean`<br>`added Byrdpip/<>c.<AfterObtained>b__27_1(Models.CardModel):System.Boolean`<br>`added Byrdpip/<AfterObtained>d__27.MoveNext():System.Void`<br>`added Byrdpip/<AfterObtained>d__27.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`added Byrdpip/<BeforeCombatStart>d__28.MoveNext():System.Void`<br>`added Byrdpip/<BeforeCombatStart>d__28.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`added Byrdpip/<SummonPet>d__32.MoveNext():System.Void`<br>`added Byrdpip/<SummonPet>d__32.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed Byrdpip/<>c.<AfterObtained>b__16_0(Models.CardModel):System.Boolean`<br>`removed Byrdpip/<>c.<AfterObtained>b__16_1(Models.CardModel):System.Boolean`<br>`removed Byrdpip/<AfterObtained>d__16.MoveNext():System.Void`<br>`removed Byrdpip/<AfterObtained>d__16.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed Byrdpip/<BeforeCombatStart>d__17.MoveNext():System.Void`<br>`removed Byrdpip/<BeforeCombatStart>d__17.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed Byrdpip/<SummonPet>d__18.MoveNext():System.Void`<br>`removed Byrdpip/<SummonPet>d__18.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `CallingBell` | `body CallingBell.GenerateRewards():System.Collections.Generic.List`1<Rewards.Reward>` |
| `DelicateFrond` | `body DelicateFrond/<BeforeCombatStart>d__2.MoveNext():System.Void` |
| `DistinguishedCape` | `body DistinguishedCape/<AfterObtained>d__9.MoveNext():System.Void` |
| `DragonFruit` | `body DragonFruit.AfterGoldGained(Entities.Players.Player):System.Threading.Tasks.Task`<br>`added DragonFruit/<AfterGoldGained>d__4.MoveNext():System.Void`<br>`added DragonFruit/<AfterGoldGained>d__4.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed DragonFruit.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed DragonFruit/<AfterGoldGained>d__5.MoveNext():System.Void`<br>`removed DragonFruit/<AfterGoldGained>d__5.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `DreamCatcher` | `body DreamCatcher.TryModifyRestSiteHealRewards(Entities.Players.Player,System.Collections.Generic.List`1<Rewards.Reward>,System.Boolean):System.Boolean` |
| `DustyTome` | `body DustyTome.SetupForPlayer(Entities.Players.Player):System.Void`<br>`body DustyTome/<AfterObtained>d__13.MoveNext():System.Void` |
| `FishingRod` | `body FishingRod.AfterCombatEnd(Rooms.CombatRoom):System.Threading.Tasks.Task` |
| `ForgottenSoul` | `body ForgottenSoul/<AfterCardExhausted>d__4.MoveNext():System.Void` |
| `FragrantMushroom` | `body FragrantMushroom/<AfterObtained>d__7.MoveNext():System.Void` |
| `FrozenEgg` | `removed FrozenEgg.IsAllowed(Runs.IRunState):System.Boolean` |
| `FurCoat` | `body FurCoat/<AfterCreatureAddedToCombat>d__27.MoveNext():System.Void` |
| `Girya` | `body Girya.AfterRoomEntered(Rooms.AbstractRoom):System.Threading.Tasks.Task`<br>`added Girya/<AfterRoomEntered>d__13.MoveNext():System.Void`<br>`added Girya/<AfterRoomEntered>d__13.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed Girya.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed Girya/<AfterRoomEntered>d__14.MoveNext():System.Void`<br>`removed Girya/<AfterRoomEntered>d__14.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `GlassEye` | `body GlassEye/<AfterObtained>d__2.MoveNext():System.Void` |
| `GremlinHorn` | `added GremlinHorn.AfterDeath(GameActions.Multiplayer.PlayerChoiceContext,Entities.Creatures.Creature,Models.CardModel,System.Boolean,System.Single):System.Threading.Tasks.Task`<br>`removed GremlinHorn.AfterDeath(GameActions.Multiplayer.PlayerChoiceContext,Entities.Creatures.Creature,System.Boolean,System.Single):System.Threading.Tasks.Task` |
| `JeweledMask` | `added JeweledMask/<>c.<BeforeHandDraw>b__2_1(Models.CardModel):System.Boolean`<br>`body JeweledMask/<BeforeHandDraw>d__2.MoveNext():System.Void` |
| `JossPaper` | `body JossPaper/<DrawIfThresholdMet>d__27.MoveNext():System.Void` |
| `JuzuBracelet` | `body JuzuBracelet.ModifyUnknownMapPointRoomTypes(System.Collections.Generic.IReadOnlySet`1<Rooms.RoomType>):System.Collections.Generic.IReadOnlySet`1<Rooms.RoomType>`<br>`removed JuzuBracelet.IsAllowed(Runs.IRunState):System.Boolean` |
| `Kaleidoscope` | `body Kaleidoscope/<AfterObtained>d__7.MoveNext():System.Void` |
| `Kusarigama` | `body Kusarigama.AfterCombatEnd(Rooms.CombatRoom):System.Threading.Tasks.Task` |
| `LargeCapsule` | `body LargeCapsule/<AfterObtained>d__5.MoveNext():System.Void` |
| `LastingCandy` | `added LastingCandy.IsAllowedInternal(Runs.IRunState):System.Boolean`<br>`added LastingCandy/<>c.<IsAllowedInternal>b__2_0(Entities.Players.Player):System.Boolean`<br>`removed LastingCandy.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed LastingCandy/<>c.<IsAllowed>b__2_0(Entities.Players.Player):System.Boolean` |
| `LeafyPoultice` | `body LeafyPoultice/<AfterObtained>d__6.MoveNext():System.Void` |
| `LetterOpener` | `body LetterOpener.AfterCombatEnd(Rooms.CombatRoom):System.Threading.Tasks.Task` |
| `LordsParasol` | `type shape changed`<br>`body LordsParasol.AfterRoomEntered(Rooms.AbstractRoom):System.Threading.Tasks.Task`<br>`added LordsParasol.get_LastPurchaseEverythingTask():System.Threading.Tasks.Task`<br>`body LordsParasol.PurchaseEverything(Entities.Merchant.MerchantInventory):System.Threading.Tasks.Task`<br>`added LordsParasol.PurchaseEverythingLocked(Entities.Merchant.MerchantInventory):System.Threading.Tasks.Task`<br>`added LordsParasol.set_LastPurchaseEverythingTask(System.Threading.Tasks.Task):System.Void`<br>`added LordsParasol/<PurchaseEverything>d__7.MoveNext():System.Void`<br>`added LordsParasol/<PurchaseEverything>d__7.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`added LordsParasol/<PurchaseEverythingLocked>d__8.MoveNext():System.Void`<br>`added LordsParasol/<PurchaseEverythingLocked>d__8.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed LordsParasol/<PurchaseEverything>d__3.MoveNext():System.Void`<br>`removed LordsParasol/<PurchaseEverything>d__3.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `LostCoffer` | `body LostCoffer/<AfterObtained>d__4.MoveNext():System.Void` |
| `LuckyFysh` | `body LuckyFysh.AfterCardChangedPiles(Models.CardModel,Entities.Cards.PileType,Models.AbstractModel):System.Threading.Tasks.Task`<br>`added LuckyFysh/<AfterCardChangedPiles>d__6.MoveNext():System.Void`<br>`added LuckyFysh/<AfterCardChangedPiles>d__6.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed LuckyFysh.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed LuckyFysh/<AfterCardChangedPiles>d__7.MoveNext():System.Void`<br>`removed LuckyFysh/<AfterCardChangedPiles>d__7.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `MassiveScroll` | `added MassiveScroll.IsAllowedInternal(Runs.IRunState):System.Boolean`<br>`removed MassiveScroll.IsAllowed(Runs.IRunState):System.Boolean` |
| `MealTicket` | `body MealTicket.AfterRoomEntered(Rooms.AbstractRoom):System.Threading.Tasks.Task`<br>`added MealTicket/<AfterRoomEntered>d__4.MoveNext():System.Void`<br>`added MealTicket/<AfterRoomEntered>d__4.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed MealTicket.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed MealTicket/<AfterRoomEntered>d__5.MoveNext():System.Void`<br>`removed MealTicket/<AfterRoomEntered>d__5.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `MembershipCard` | `body MembershipCard.ModifyMerchantPrice(Entities.Players.Player,Entities.Merchant.MerchantEntry,System.Decimal):System.Decimal` |
| `MoltenEgg` | `removed MoltenEgg.IsAllowed(Runs.IRunState):System.Boolean` |
| `NeowsBones` | `body NeowsBones.<AfterObtained>b__7_0(Models.RelicModel):Rewards.Reward`<br>`body NeowsBones/<AfterObtained>d__7.MoveNext():System.Void` |
| `NewLeaf` | `body NewLeaf/<AfterObtained>d__8.MoveNext():System.Void` |
| `OldCoin` | `body OldCoin.AfterObtained():System.Threading.Tasks.Task`<br>`added OldCoin/<AfterObtained>d__8.MoveNext():System.Void`<br>`added OldCoin/<AfterObtained>d__8.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed OldCoin.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed OldCoin/<AfterObtained>d__9.MoveNext():System.Void`<br>`removed OldCoin/<AfterObtained>d__9.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `Orrery` | `body Orrery/<AfterObtained>d__6.MoveNext():System.Void` |
| `PaelsEye` | `added PaelsEye.ShouldTakeExtraTurnEarly(Entities.Players.Player):System.Boolean`<br>`body PaelsEye/<BeforeSideTurnEndEarly>d__16.MoveNext():System.Void`<br>`removed PaelsEye.ShouldTakeExtraTurn(Entities.Players.Player):System.Boolean` |
| `PaelsLegion` | `body PaelsLegion.ModifyBlockMultiplicative(Entities.Creatures.Creature,System.Decimal,ValueProps.ValueProp,Models.CardModel,Entities.Cards.CardPlay):System.Decimal` |
| `PaelsWing` | `body PaelsWing/<OnSacrifice>d__19.MoveNext():System.Void` |
| `PandorasBox` | `body PandorasBox.<AfterObtained>b__4_1(Models.CardModel):Entities.Cards.CardTransformation` |
| `ParryingShield` | `body ParryingShield/<AfterSideTurnEnd>d__4.MoveNext():System.Void` |
| `Permafrost` | `body Permafrost/<AfterCardPlayed>d__9.MoveNext():System.Void` |
| `PhialHolster` | `body PhialHolster/<AfterObtained>d__8.MoveNext():System.Void` |
| `Planisphere` | `body Planisphere.AfterRoomEntered(Rooms.AbstractRoom):System.Threading.Tasks.Task`<br>`added Planisphere/<AfterRoomEntered>d__4.MoveNext():System.Void`<br>`added Planisphere/<AfterRoomEntered>d__4.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed Planisphere.IsAllowed(Runs.IRunState):System.Boolean`<br>`removed Planisphere/<AfterRoomEntered>d__5.MoveNext():System.Void`<br>`removed Planisphere/<AfterRoomEntered>d__5.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `PotionBelt` | `body PotionBelt.AfterObtained():System.Threading.Tasks.Task`<br>`added PotionBelt.IsAllowedInternal(Runs.IRunState):System.Boolean`<br>`added PotionBelt/<AfterObtained>d__8.MoveNext():System.Void`<br>`added PotionBelt/<AfterObtained>d__8.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed PotionBelt/<AfterObtained>d__7.MoveNext():System.Void`<br>`removed PotionBelt/<AfterObtained>d__7.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void` |
| `PowerCell` | `added PowerCell/<>c.<BeforeSideTurnStart>b__4_1(Models.CardModel):System.Boolean`<br>`added PowerCell/<>c.<BeforeSideTurnStart>b__4_2(Models.CardModel):System.Boolean`<br>`body PowerCell/<BeforeSideTurnStart>d__4.MoveNext():System.Void` |
| `PrayerWheel` | `body PrayerWheel.TryModifyRewards(Entities.Players.Player,System.Collections.Generic.List`1<Rewards.Reward>,Rooms.AbstractRoom):System.Boolean` |
| `RainbowRing` | `body RainbowRing/<AfterCardPlayed>d__23.MoveNext():System.Void` |
| `Regalite` | `body Regalite.get_CanonicalVars():System.Collections.Generic.IEnumerable`1<Localization.DynamicVars.DynamicVar>` |
| `RingingTriangle` | `added RingingTriangle.AfterRoomEntered(Rooms.AbstractRoom):System.Threading.Tasks.Task`<br>`added RingingTriangle/<AfterRoomEntered>d__4.MoveNext():System.Void`<br>`added RingingTriangle/<AfterRoomEntered>d__4.SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine):System.Void`<br>`removed RingingTriangle.ShouldFlush(Entities.Players.Player):System.Boolean` |
| `RoyalStamp` | `body RoyalStamp/<AfterObtained>d__8.MoveNext():System.Void` |
| `SandCastle` | `body SandCastle.AfterObtained():System.Threading.Tasks.Task` |
| `ScrollBoxes` | `added ScrollBoxes.GenerateRandomBundles(Entities.Players.Player,Random.Rng):System.Collections.Generic.List`1<System.Collections.Generic.IReadOnlyList`1<Models.CardModel>>`<br>`body ScrollBoxes/<AfterObtained>d__4.MoveNext():System.Void`<br>`removed ScrollBoxes.GenerateRandomBundles(Entities.Players.Player):System.Collections.Generic.List`1<System.Collections.Generic.IReadOnlyList`1<Models.CardModel>>` |
| `SereTalon` | `body SereTalon/<AfterObtained>d__10.MoveNext():System.Void` |
| `Shovel` | `removed Shovel.IsAllowed(Runs.IRunState):System.Boolean` |
| `SilverCrucible` | `added SilverCrucible.IsAllowedInternal(Runs.IRunState):System.Boolean`<br>`removed SilverCrucible.IsAllowed(Runs.IRunState):System.Boolean` |
| `Tingsha` | `body Tingsha/<AfterCardDiscarded>d__4.MoveNext():System.Void` |
| `ToastyMittens` | `body ToastyMittens/<AfterPlayerTurnStart>d__6.MoveNext():System.Void` |
| `TouchOfOrobas` | `body TouchOfOrobas.get_RefinementUpgrades():System.Collections.Generic.Dictionary`2<Models.ModelId,Models.RelicModel>` |
| `ToxicEgg` | `removed ToxicEgg.IsAllowed(Runs.IRunState):System.Boolean` |
| `ToyBox` | `body ToyBox/<AfterObtained>d__24.MoveNext():System.Void` |
| `VelvetChoker` | `type shape changed`<br>`added VelvetChoker.<GetCardsPlayedThisTurn>b__12_0(Combat.History.Entries.CardPlayStartedEntry):System.Boolean`<br>`body VelvetChoker.AfterCardPlayed(GameActions.Multiplayer.PlayerChoiceContext,Entities.Cards.CardPlay):System.Threading.Tasks.Task`<br>`body VelvetChoker.AfterCombatEnd(Rooms.CombatRoom):System.Threading.Tasks.Task`<br>`body VelvetChoker.AfterRoomEntered(Rooms.AbstractRoom):System.Threading.Tasks.Task`<br>`body VelvetChoker.BeforeSideTurnStart(GameActions.Multiplayer.PlayerChoiceContext,Combat.CombatSide,System.Collections.Generic.IReadOnlyList`1<Entities.Creatures.Creature>,Combat.ICombatState):System.Threading.Tasks.Task`<br>`body VelvetChoker.get_DisplayAmount():System.Int32`<br>`body VelvetChoker.get_ShouldPreventCardPlay():System.Boolean`<br>`added VelvetChoker.GetCardsPlayedThisTurn():System.Int32` |
| `WarHammer` | `body WarHammer.AfterCombatVictory(Rooms.CombatRoom):System.Threading.Tasks.Task` |
| `WarPaint` | `body WarPaint.AfterObtained():System.Threading.Tasks.Task` |
| `Whetstone` | `body Whetstone.AfterObtained():System.Threading.Tasks.Task` |
| `WhisperingEarring` | `body WhisperingEarring/<AfterAutoPrePlayPhaseEnteredLate>d__8.MoveNext():System.Void` |
| `WhiteBeastStatue` | `removed WhiteBeastStatue.IsAllowed(Runs.IRunState):System.Boolean` |
| `WhiteStar` | `body WhiteStar.TryModifyRewards(Entities.Players.Player,System.Collections.Generic.List`1<Rewards.Reward>,Rooms.AbstractRoom):System.Boolean`<br>`removed WhiteStar.IsAllowed(Runs.IRunState):System.Boolean` |
| `WingCharm` | `body WingCharm.TryModifyCardRewardOptionsLate(Entities.Players.Player,System.Collections.Generic.List`1<Entities.Cards.CardCreationResult>,Runs.CardCreationOptions):System.Boolean` |
| `WingedBoots` | `added WingedBoots.IsAllowedInternal(Runs.IRunState):System.Boolean`<br>`removed WingedBoots.IsAllowed(Runs.IRunState):System.Boolean` |
| `YummyCookie` | `body YummyCookie.get_IconBaseName():System.String` |
