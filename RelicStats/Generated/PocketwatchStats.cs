using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PocketwatchStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Pocketwatch";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}