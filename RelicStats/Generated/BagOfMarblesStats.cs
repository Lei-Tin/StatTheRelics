using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BagOfMarblesStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BagOfMarbles";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Vulnerable Applied" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var vulnerable = counters.TryGetValue("Vulnerable Applied", out var v) ? v : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Vulnerable Applied: {vulnerable}");
            return sb.ToString().TrimEnd();
        }
    }
}
