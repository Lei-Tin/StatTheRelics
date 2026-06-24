using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PhialHolsterStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PhialHolster";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Potion Slots Gained" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var potions = textStats != null && textStats.TryGetValue("Potions", out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Potion Slots Gained: {(counters.TryGetValue("Potion Slots Gained", out var slots) ? slots : 0)}");
            sb.AppendLine();
            sb.AppendLine("Potions:");
            sb.Append(potions);
            return sb.ToString().TrimEnd();
        }
    }
}
