using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class LavaLampStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.LavaLamp";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}