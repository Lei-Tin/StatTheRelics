using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DiamondDiademStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DiamondDiadem";
        public override IReadOnlyList<string> DefaultCounters => new [] {
            "Times Triggered",
            "Damage Prevented"
        };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            return FormatDefault(DefaultCounters, FilterDisplayedCounters(counters), historyMode, bannerNote);
        }

        static IReadOnlyDictionary<string,int> FilterDisplayedCounters(IReadOnlyDictionary<string,int> counters) {
            var result = new Dictionary<string, int>();
            foreach (var key in DefaultKeys) {
                if (counters != null && counters.TryGetValue(key, out var value)) result[key] = value;
            }
            return result;
        }

        static readonly IReadOnlyList<string> DefaultKeys = new [] {
            "Times Triggered",
            "Damage Prevented"
        };
    }
}
