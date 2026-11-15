using System.Collections.Generic;
using System.Linq;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ConductorsBatonStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.ConductorsBaton";
        public override IReadOnlyList<string> DefaultCounters => new[] { "Damage Mitigated" };

        public override string Format(IReadOnlyDictionary<string, int> counters, bool historyMode, string bannerNote) =>
            FormatDefault(DefaultCounters, counters.Where(pair => DefaultCounters.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value), historyMode, bannerNote);
    }
}
