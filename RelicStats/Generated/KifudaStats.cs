using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class KifudaStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Kifuda";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Enchanted Cards Played" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var enchantedCardsPlayed = counters.TryGetValue("Enchanted Cards Played", out var c) ? c : 0;
            var cards = textStats != null && textStats.TryGetValue("Cards Enchanted", out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Enchanted Cards Played: {enchantedCardsPlayed}");
            sb.AppendLine();
            sb.AppendLine("Cards Enchanted:");
            sb.Append(cards);
            return sb.ToString().TrimEnd();
        }
    }
}
