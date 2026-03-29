using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BloodVialStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BloodVial";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}