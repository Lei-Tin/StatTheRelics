using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DeprecatedRelicStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DeprecatedRelic";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            return FormatNoStats(historyMode, bannerNote);
        }
    }
}
