using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PreservedFogStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PreservedFog";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Removed" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var cardsRemoved = textStats != null && textStats.TryGetValue("Cards Removed", out var removed) && !string.IsNullOrWhiteSpace(removed)
                ? removed
                : "None";
            var curseAdded = textStats != null && textStats.TryGetValue("Curse Added", out var curse) && !string.IsNullOrWhiteSpace(curse)
                ? curse
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Curse Added:");
            sb.AppendLine(curseAdded);
            sb.AppendLine();
            sb.AppendLine("Cards Removed:");
            sb.Append(cardsRemoved);
            return sb.ToString().TrimEnd();
        }
    }
}
