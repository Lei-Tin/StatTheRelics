using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class StrikeDummyStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.StrikeDummy";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}