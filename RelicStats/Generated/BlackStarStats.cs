using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BlackStarStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BlackStar";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Relics Given" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var relicsGiven = counters.TryGetValue("Relics Given", out var r) ? r : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Relics Given: {relicsGiven}");
            return sb.ToString().TrimEnd();
        }
    }
}