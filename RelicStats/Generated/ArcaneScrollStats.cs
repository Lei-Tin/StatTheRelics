using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ArcaneScrollStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.ArcaneScroll";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}