using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DollysMirrorStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DollysMirror";
        public override IReadOnlyList<string> DefaultCounters => new string[] { };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var prefix = historyMode && !string.IsNullOrEmpty(bannerNote) ? bannerNote + "\n" : string.Empty;
            var card = textStats != null && textStats.TryGetValue("Duplicated Card", out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "Unknown";

            return $"{prefix}Duplicated Card:\n{card}".TrimEnd();
        }
    }
}
