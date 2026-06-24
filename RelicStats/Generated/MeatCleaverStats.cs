using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class MeatCleaverStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.MeatCleaver";
        public override IReadOnlyList<string> DefaultCounters => new [] {
            "Cards Removed",
            "Max HP Gained"
        };
    }
}
