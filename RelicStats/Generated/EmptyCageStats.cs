using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class EmptyCageStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.EmptyCage";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Removed" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var body = FormatDefault(DefaultCounters, counters, historyMode, bannerNote);
            if (textStats == null || !textStats.TryGetValue("Cards Removed", out var cards) || string.IsNullOrWhiteSpace(cards)) {
                return body;
            }

            return $"{body}\n\nCards Removed:\n{cards}".TrimEnd();
        }
    }
}
