using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;
using System;
using System.Linq;
using System.Collections;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BoneFluteStats : BaseRelicStats {
        static int? cachedBlockPerFlash;

        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BoneFlute";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();

            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var blockPerFlash = ResolveBlockPerFlash();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Block Given: {flashes * blockPerFlash}");
            return sb.ToString().TrimEnd();
        }

        static int ResolveBlockPerFlash() {
            if (cachedBlockPerFlash.HasValue) return cachedBlockPerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.BoneFlute", false))
                    .FirstOrDefault(t => t != null);

                if (type == null) {
                    cachedBlockPerFlash = 0;
                    return 0;
                }

                var relic = Activator.CreateInstance(type, true);
                if (relic == null) {
                    cachedBlockPerFlash = 0;
                    return 0;
                }

                var canonicalVars = ReflectionUtil.GetMemberValue(relic, "CanonicalVars") as IEnumerable;
                if (canonicalVars == null) {
                    cachedBlockPerFlash = 0;
                    return 0;
                }

                foreach (var dv in canonicalVars) {
                    if (dv == null) continue;
                    var raw = ReflectionUtil.GetMemberValue(dv, "BaseValue");
                    if (raw == null) continue;

                    var blockPerFlash = Math.Max(0, Convert.ToInt32(raw));
                    cachedBlockPerFlash = blockPerFlash;
                    return blockPerFlash;
                }

                cachedBlockPerFlash = 0;
                return 0;
            } catch {
                cachedBlockPerFlash = 0;
                return 0;
            }
        }
    }
}