using System.Collections.Generic;
using System;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class AlchemicalCofferStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.AlchemicalCoffer";
        public override IReadOnlyList<string> DefaultCounters => Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            return "No stats are available for this relic";
        }
    }
}