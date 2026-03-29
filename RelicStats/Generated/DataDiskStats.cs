using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DataDiskStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DataDisk";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}