using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;
using System;
using System.Linq;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BigHatStats : BaseRelicStats {
        static int? cachedCardsPerFlash;

        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BigHat";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();

            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var cardsPerFlash = ResolveCardsPerFlash();
            var etherealCardsGiven = flashes * cardsPerFlash;
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Ethereal Cards Given: {etherealCardsGiven}");
            return sb.ToString().TrimEnd();
        }

        static int ResolveCardsPerFlash() {
            if (cachedCardsPerFlash.HasValue) return cachedCardsPerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.BigHat", false))
                    .FirstOrDefault(t => t != null);

                if (type == null) {
                    cachedCardsPerFlash = 0;
                    return 0;
                }

                var relic = Activator.CreateInstance(type, true);
                if (relic == null) {
                    cachedCardsPerFlash = 0;
                    return 0;
                }

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                var cardsVar = ReflectionUtil.GetMemberValue(dynamicVars, "Cards");
                var intValueRaw = ReflectionUtil.GetMemberValue(cardsVar, "IntValue");
                var cardsPerFlash = intValueRaw == null ? 0 : Math.Max(0, Convert.ToInt32(intValueRaw));

                cachedCardsPerFlash = cardsPerFlash;
                return cardsPerFlash;
            } catch {
                cachedCardsPerFlash = 0;
                return 0;
            }
        }
    }
}