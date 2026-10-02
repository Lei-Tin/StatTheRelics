using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    public static class GhostSeedPatch {
        internal static void CountEtherealExhaust(GhostSeed relic, CardModel card) {
            try {
                if (relic == null || card == null || !IsBasicStrikeOrDefend(card)) return;
                RelicTracker.AddAmount(relic, "Strike & Defends Exhausted (from ethereal)", 1);
            } catch { }
        }

        static bool IsBasicStrikeOrDefend(CardModel card) {
            try {
                if (card.Rarity != CardRarity.Basic) return false;
                return card.Tags != null && (card.Tags.Contains(CardTag.Strike) || card.Tags.Contains(CardTag.Defend));
            } catch {
                return false;
            }
        }
    }

    // CardCmd.Exhaust performs the actual pile move and then calls this hook.
    // Counting here avoids depending on the outer async Exhaust task reaching
    // a successful continuation after all other exhaust listeners finish.
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardExhausted), new Type[] {
        typeof(ICombatState),
        typeof(PlayerChoiceContext),
        typeof(CardModel),
        typeof(bool)
    })]
    public static class GhostSeedEtherealExhaustPatch {
        static void Prefix(CardModel card, bool causedByEthereal) {
            try {
                if (!causedByEthereal || card == null || card.Owner == null) return;
                var relic = ReflectionUtil.FindRelic<GhostSeed>(card.Owner);
                if (relic == null) return;
                GhostSeedPatch.CountEtherealExhaust(relic, card);
            } catch { }
        }
    }
}
