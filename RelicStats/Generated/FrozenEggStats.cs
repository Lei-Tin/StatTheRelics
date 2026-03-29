using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FrozenEggStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FrozenEgg";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}