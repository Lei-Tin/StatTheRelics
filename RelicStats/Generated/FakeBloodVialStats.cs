using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FakeBloodVialStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FakeBloodVial";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}