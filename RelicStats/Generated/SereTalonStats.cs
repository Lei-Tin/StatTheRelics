using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class SereTalonStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.SereTalon";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Wishes Played" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var curses = textStats != null && textStats.TryGetValue("Curses Added", out var curseValue) && !string.IsNullOrWhiteSpace(curseValue)
                ? curseValue
                : "None";
            var wishesPlayed = counters != null && counters.TryGetValue("Wishes Played", out var wishesValue) ? wishesValue : 0;

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Wishes Played: {wishesPlayed}");
            sb.AppendLine();
            sb.AppendLine("Curses Added:");
            sb.Append(curses);
            return sb.ToString().TrimEnd();
        }
    }
}
