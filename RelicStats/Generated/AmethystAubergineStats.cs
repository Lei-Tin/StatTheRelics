using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class AmethystAubergineStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.AmethystAubergine";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Gold Generated" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var money = counters.TryGetValue("Gold Generated", out var m) ? m : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Money Generated: {money}");
            return sb.ToString().TrimEnd();
        }
    }
}