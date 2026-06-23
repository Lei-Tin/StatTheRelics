using System;
using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class AlchemicalCofferStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.AlchemicalCoffer";
        public override IReadOnlyList<string> DefaultCounters => Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var potions = textStats != null && textStats.TryGetValue("Potions", out var p) && !string.IsNullOrWhiteSpace(p)
                ? p
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Potions:");
            sb.AppendLine(potions);
            return sb.ToString().TrimEnd();
        }
    }
}
