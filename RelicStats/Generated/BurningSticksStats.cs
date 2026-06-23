using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BurningSticksStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BurningSticks";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Generated" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var cards = counters.TryGetValue("Cards Generated", out var c) ? c : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Cards Generated: {cards}");
            return sb.ToString().TrimEnd();
        }
    }
}
