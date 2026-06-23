using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BrimstoneStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Brimstone";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Strength Gained", "Enemy Strength Given" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var strength = counters.TryGetValue("Strength Gained", out var s) ? s : 0;
            var enemyStrength = counters.TryGetValue("Enemy Strength Given", out var e) ? e : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Strength Gained: {strength}");
            sb.AppendLine($"Enemy Strength Given: {enemyStrength}");
            return sb.ToString().TrimEnd();
        }
    }
}
