using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class JewelryBoxStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.JewelryBox";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}