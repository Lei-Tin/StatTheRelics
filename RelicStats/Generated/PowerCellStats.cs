using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PowerCellStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PowerCell";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}