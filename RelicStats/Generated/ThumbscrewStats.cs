using System.Collections.Generic;
using System.Linq;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ThumbscrewStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Thumbscrew";
        public override IReadOnlyList<string> DefaultCounters => new[] { "Extra Hits Executed" };

        public override string Format(IReadOnlyDictionary<string, int> counters, bool historyMode, string bannerNote) =>
            FormatDefault(DefaultCounters, counters.Where(pair => DefaultCounters.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value), historyMode, bannerNote);
    }
}
