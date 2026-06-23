using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DistinguishedCapeStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DistinguishedCape";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Apparitions Played", "Max HP Lost", "Cards Added" };
    }
}
