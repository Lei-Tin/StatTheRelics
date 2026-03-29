using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FakeStrikeDummyStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FakeStrikeDummy";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}