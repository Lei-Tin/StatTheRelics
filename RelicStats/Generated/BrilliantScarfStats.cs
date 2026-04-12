using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BrilliantScarfStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BrilliantScarf";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Freed Cards" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            var freedCards = counters.TryGetValue("Freed Cards", out var c) ? c : 0;
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Freed Cards: {freedCards}");
            return sb.ToString().TrimEnd();
        }
    }
}