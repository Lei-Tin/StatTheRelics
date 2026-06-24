using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PrecariousShearsStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PrecariousShears";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Damage Taken", "Cards Removed" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var damageTaken = counters.TryGetValue("Damage Taken", out var d) ? d : 0;
            var cardsRemoved = textStats != null && textStats.TryGetValue("Cards Removed", out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Damage Taken: {damageTaken}");
            sb.AppendLine();
            sb.AppendLine("Cards Removed:");
            sb.Append(cardsRemoved);
            return sb.ToString().TrimEnd();
        }
    }
}
