using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BigMushroomStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BigMushroom";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Card Drawn Reduced" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var cardsReduced = counters.TryGetValue("Card Drawn Reduced", out var m) ? m : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Card Drawn Reduced: {cardsReduced}");
            return sb.ToString().TrimEnd();
        }
    }
}