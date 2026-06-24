using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PaperPhrogStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PaperPhrog";
        public override string Format(System.Collections.Generic.IReadOnlyDictionary<string,int> counters, System.Collections.Generic.IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote)
            => FormatNoStats(historyMode, bannerNote);
    }
}
