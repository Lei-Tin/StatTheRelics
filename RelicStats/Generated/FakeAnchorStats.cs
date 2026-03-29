using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FakeAnchorStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FakeAnchor";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}