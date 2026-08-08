using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class NeowsSacrificeStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.NeowsSacrifice";
        public override IReadOnlyList<string> DefaultCounters => new[] { "HP Healed" };

        public override string Format(IReadOnlyDictionary<string, int> counters, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            var healed = counters.TryGetValue("HP Healed", out var value) ? value : 0;
            sb.Append($"HP Healed: {healed}");
            return sb.ToString();
        }
    }
}
