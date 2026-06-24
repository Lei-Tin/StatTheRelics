using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class WongoCustomerAppreciationBadgeStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.WongoCustomerAppreciationBadge";

        public override System.Collections.Generic.IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(
            System.Collections.Generic.IReadOnlyDictionary<string,int> counters,
            System.Collections.Generic.IReadOnlyDictionary<string,string> textStats,
            bool historyMode,
            string bannerNote
        ) {
            return FormatNoStats(historyMode, bannerNote);
        }
    }
}
