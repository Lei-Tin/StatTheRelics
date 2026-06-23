using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ByrdpipStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Byrdpip";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Byrd Swoops Played" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var played = counters.TryGetValue("Byrd Swoops Played", out var p) ? p : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Byrd Swoops Played: {played}");
            return sb.ToString().TrimEnd();
        }
    }
}
