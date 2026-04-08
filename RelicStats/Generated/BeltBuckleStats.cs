using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BeltBuckleStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BeltBuckle";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Times Dexterity Applied" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var timesApplied = counters.TryGetValue("Times Dexterity Applied", out var m) ? m : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Times Dexterity Applied: {timesApplied}");
            return sb.ToString().TrimEnd();
        }
    }
}