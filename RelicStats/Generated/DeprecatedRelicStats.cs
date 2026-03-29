using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DeprecatedRelicStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DeprecatedRelic";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}