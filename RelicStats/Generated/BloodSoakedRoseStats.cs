using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BloodSoakedRoseStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BloodSoakedRose";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Energy Given" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var energyGiven = counters.TryGetValue("Energy Given", out var e) ? e : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Energy Given: {energyGiven}");
            return sb.ToString().TrimEnd();

            // TODO: Also add tracking for the card generated (enthralled), how many times it is played
        }
    }
}