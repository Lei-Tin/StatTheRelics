using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BigHatStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BigHat";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();

            // It actually flashes twice for some reason from the decompiled code
            // Since it gives 2 cards, we will just use flashes as the counter for ethereal cards given
            var etherealCardsGiven = counters.TryGetValue("Flashes", out var e) ? e : 0;
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Ethereal Cards Given: {etherealCardsGiven}");
            return sb.ToString().TrimEnd();
        }
    }
}