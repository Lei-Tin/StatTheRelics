using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FakeOrichalcumStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FakeOrichalcum";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}