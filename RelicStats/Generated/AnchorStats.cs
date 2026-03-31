using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class AnchorStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Anchor";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Block Generated" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var block = counters.TryGetValue("Block Generated", out var m) ? m : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Block Generated: {block}");
            return sb.ToString().TrimEnd();
        }
    }
}