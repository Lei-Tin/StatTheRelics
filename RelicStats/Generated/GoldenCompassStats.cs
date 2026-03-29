using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class GoldenCompassStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.GoldenCompass";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}