using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class RoyalStampStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.RoyalStamp";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Enchanted Cards Played" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var played = counters != null && counters.TryGetValue("Enchanted Cards Played", out var playedValue) ? playedValue : 0;
            var cards = textStats != null && textStats.TryGetValue("Cards Enchanted", out var cardValue) && !string.IsNullOrWhiteSpace(cardValue)
                ? cardValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Enchanted Cards Played: {played}");
            sb.AppendLine();
            sb.AppendLine("Card Enchanted:");
            sb.Append(cards);
            return sb.ToString().TrimEnd();
        }
    }
}
