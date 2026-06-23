using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FishingRodStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FishingRod";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Normal Combats", "Cards Upgraded" };
    }
}
