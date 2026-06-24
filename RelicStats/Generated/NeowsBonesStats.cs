using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class NeowsBonesStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.NeowsBones";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var relicsOffered = GetText(textStats, "Relics Offered");
            var curseAdded = GetText(textStats, "Curse Added");

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Relics Offered:");
            sb.AppendLine(relicsOffered);
            sb.AppendLine();
            sb.AppendLine("Curse Added:");
            sb.Append(curseAdded);
            return sb.ToString().TrimEnd();
        }

        static string GetText(IReadOnlyDictionary<string,string> textStats, string key) {
            return textStats != null && textStats.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "None";
        }
    }
}
