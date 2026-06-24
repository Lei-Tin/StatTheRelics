using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class NewLeafStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.NewLeaf";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var transformed = GetText(textStats, "Cards Transformed");
            var obtained = GetText(textStats, "Cards Obtained");

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Cards Transformed:");
            sb.AppendLine(transformed);
            sb.AppendLine();
            sb.AppendLine("Cards Obtained:");
            sb.Append(obtained);
            return sb.ToString().TrimEnd();
        }

        static string GetText(IReadOnlyDictionary<string,string> textStats, string key) {
            return textStats != null && textStats.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "None";
        }
    }
}
