using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PendulumStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Pendulum";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}