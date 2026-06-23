using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FakeMerchantsRugStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FakeMerchantsRug";
        public override IReadOnlyList<string> DefaultCounters => new string[] { };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var prefix = historyMode && !string.IsNullOrEmpty(bannerNote) ? bannerNote + "\n" : string.Empty;
            return $"{prefix}Stats are not available for this relic.";
        }
    }
}
