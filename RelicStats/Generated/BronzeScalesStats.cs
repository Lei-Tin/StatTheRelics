using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BronzeScalesStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BronzeScales";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Thorns Damage" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var thorns = counters.TryGetValue("Thorns Damage", out var t) ? t : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Thorns Damage: {thorns}");
            return sb.ToString().TrimEnd();
        }
    }
}
