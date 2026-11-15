using System.Collections.Generic;
using System.Linq;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class OcarinaStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Ocarina";
        public override IReadOnlyList<string> DefaultCounters => new[] { "Wah Cards Generated", "Cho Cards Generated", "Ki Cards Generated" };

        public override string Format(IReadOnlyDictionary<string, int> counters, bool historyMode, string bannerNote) =>
            FormatDefault(DefaultCounters, counters.Where(pair => DefaultCounters.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value), historyMode, bannerNote);
    }
}
