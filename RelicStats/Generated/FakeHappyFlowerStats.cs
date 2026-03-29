using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FakeHappyFlowerStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FakeHappyFlower";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}