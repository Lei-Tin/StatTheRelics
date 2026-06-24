using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class SilkenTressStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.SilkenTress";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Gold Lost" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var gold = counters != null && counters.TryGetValue("Gold Lost", out var goldValue) ? goldValue : 0;
            var cards = textStats != null && textStats.TryGetValue("Card Selected", out var cardValue) && !string.IsNullOrWhiteSpace(cardValue)
                ? cardValue
                : "None";
            var rewards = textStats != null && textStats.TryGetValue("Card Rewards", out var rewardValue) && !string.IsNullOrWhiteSpace(rewardValue)
                ? rewardValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Gold Lost: {gold}");
            sb.AppendLine("Card Rewards:");
            sb.AppendLine(rewards);
            sb.AppendLine();
            sb.AppendLine("Card Selected:");
            sb.Append(cards);
            return sb.ToString().TrimEnd();
        }
    }
}
