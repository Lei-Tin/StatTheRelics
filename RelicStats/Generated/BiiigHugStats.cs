using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BiiigHugStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BiiigHug";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Soots Added" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sootsAdded = counters.TryGetValue("Soots Added", out var s) ? s : 0;
            var removed = textStats != null && textStats.TryGetValue("Cards Removed", out var r) && !string.IsNullOrWhiteSpace(r)
                ? r
                : "Unknown";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Soots Added: {sootsAdded}");
            sb.AppendLine();
            sb.AppendLine("Cards Removed:");
            sb.AppendLine(removed);
            return sb.ToString().TrimEnd();
        }
    }
}
