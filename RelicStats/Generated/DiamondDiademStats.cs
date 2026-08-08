using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DiamondDiademStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DiamondDiadem";
        public override IReadOnlyList<string> DefaultCounters => UsesBlockEffect ? BlockCounters : DamageCounters;

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            return FormatDefault(DefaultCounters, FilterDisplayedCounters(counters), historyMode, bannerNote);
        }

        static IReadOnlyDictionary<string,int> FilterDisplayedCounters(IReadOnlyDictionary<string,int> counters) {
            var result = new Dictionary<string, int>();
            foreach (var key in UsesBlockEffect ? BlockCounters : DamageCounters) {
                if (counters != null && counters.TryGetValue(key, out var value)) result[key] = value;
            }
            return result;
        }

        static bool UsesBlockEffect => typeof(MegaCrit.Sts2.Core.Models.Relics.DiamondDiadem)
            .GetMethod("AfterSideTurnStart", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly) != null;

        static readonly IReadOnlyList<string> DamageCounters = new [] {
            "Times Triggered",
            "Damage Prevented"
        };

        static readonly IReadOnlyList<string> BlockCounters = new [] {
            "Times Triggered",
            "Block Gained"
        };
    }
}
