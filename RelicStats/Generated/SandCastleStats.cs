using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class SandCastleStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.SandCastle";
        public override IReadOnlyList<string> DefaultCounters => new string[0];

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var cards = textStats != null && textStats.TryGetValue("Cards Upgraded", out var cardValue) && !string.IsNullOrWhiteSpace(cardValue)
                ? cardValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Cards Upgraded:");
            sb.Append(cards);
            return sb.ToString().TrimEnd();
        }
    }
}
