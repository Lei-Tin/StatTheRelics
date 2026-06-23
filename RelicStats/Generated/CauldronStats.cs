using System;
using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class CauldronStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Cauldron";
        public override IReadOnlyList<string> DefaultCounters => Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var offered = textStats != null && textStats.TryGetValue("Potions Offered", out var o) && !string.IsNullOrWhiteSpace(o)
                ? o
                : "None";
            var selected = textStats != null && textStats.TryGetValue("Selected Potions", out var s) && !string.IsNullOrWhiteSpace(s)
                ? s
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Potions Offered:");
            sb.AppendLine(offered);
            sb.AppendLine();
            sb.AppendLine("Selected Potions:");
            sb.AppendLine(selected);
            return sb.ToString().TrimEnd();
        }
    }
}
