using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FakeSneckoEyeStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FakeSneckoEye";
        public override IReadOnlyList<string> DefaultCounters => new [] { "0 Cost", "1 Cost", "2 Cost", "3 Cost" };
    }
}
