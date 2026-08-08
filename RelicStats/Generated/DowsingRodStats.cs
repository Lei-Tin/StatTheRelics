using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DowsingRodStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DowsingRod";
        public override IReadOnlyList<string> DefaultCounters => new[] { "Abundance Played" };

        public override string Format(IReadOnlyDictionary<string, int> counters, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.Append($"Abundance Played: {GetCounter(counters, "Abundance Played")}");
            return sb.ToString();
        }

        static int GetCounter(IReadOnlyDictionary<string, int> counters, string key) {
            return counters.TryGetValue(key, out var value) ? value : 0;
        }
    }
}
