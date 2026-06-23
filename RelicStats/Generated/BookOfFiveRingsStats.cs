using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BookOfFiveRingsStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BookOfFiveRings";
        public override IReadOnlyList<string> DefaultCounters => new [] { "HP Healed" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var hpHealed = counters.TryGetValue("HP Healed", out var h) ? h : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"HP Healed: {hpHealed}");
            return sb.ToString().TrimEnd();
        }
    }
}
