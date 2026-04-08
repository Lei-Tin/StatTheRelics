using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BeautifulBraceletStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BeautifulBracelet";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Swift Cards Played" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var swiftCardsPlayed = counters.TryGetValue("Swift Cards Played", out var m) ? m : 0;
            var swiftCardsEnchanted = textStats != null && textStats.TryGetValue("Swift Cards Enchanted", out var s) && !string.IsNullOrWhiteSpace(s)
                ? s
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Swift Cards Enchanted:");
            sb.AppendLine(swiftCardsEnchanted);
            sb.AppendLine();
            sb.AppendLine($"Swift Cards Played: {swiftCardsPlayed}");
            return sb.ToString().TrimEnd();
        }
    }
}