using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ElectricShrympStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.ElectricShrymp";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Auto Played Card" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var body = FormatDefault(DefaultCounters, counters, historyMode, bannerNote);
            var card = textStats != null && textStats.TryGetValue("Card Enchanted", out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "Unknown";

            return $"{body}\n\nCard Enchanted:\n{card}".TrimEnd();
        }
    }
}
