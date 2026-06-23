using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class AkabekoStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Akabeko";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Vigor Given" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var vigorGiven = counters.TryGetValue("Vigor Given", out var v) ? v : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Vigor Given: {vigorGiven}");
            return sb.ToString().TrimEnd();
        }
    }
}
