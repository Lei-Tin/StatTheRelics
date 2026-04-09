using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BlackBloodStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BlackBlood";
        public override IReadOnlyList<string> DefaultCounters => new [] { "HP Healed" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var healing = counters.TryGetValue("HP Healed", out var h) ? h : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"HP Healed: {healing}");
            return sb.ToString().TrimEnd();
        }
    }
}