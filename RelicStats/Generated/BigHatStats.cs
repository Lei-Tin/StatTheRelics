using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BigHatStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BigHat";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Ethereal Cards Given" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var etherealCardsGiven = counters.TryGetValue("Ethereal Cards Given", out var c) ? c : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Ethereal Cards Given: {etherealCardsGiven}");
            return sb.ToString().TrimEnd();
        }
    }
}
