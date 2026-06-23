using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DingyRugStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DingyRug";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Colorless Cards Offered" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var displayed = new Dictionary<string, int>();
            if (counters != null && counters.TryGetValue("Colorless Cards Offered", out var value)) {
                displayed["Colorless Cards Offered"] = value;
            }

            return FormatDefault(DefaultCounters, displayed, historyMode, bannerNote);
        }
    }
}
