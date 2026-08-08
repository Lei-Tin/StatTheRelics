using System.Collections.Generic;
using System.Globalization;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class SneckoEyeStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.SneckoEye";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Drawn", "0 Cost", "1 Cost", "2 Cost", "3 Cost" };

        public override string Format(IReadOnlyDictionary<string, int> counters, bool historyMode, string bannerNote) {
            var zeroCost = counters.TryGetValue("0 Cost", out var zero) ? zero : 0;
            var oneCost = counters.TryGetValue("1 Cost", out var one) ? one : 0;
            var twoCost = counters.TryGetValue("2 Cost", out var two) ? two : 0;
            var threeCost = counters.TryGetValue("3 Cost", out var three) ? three : 0;
            var randomizedCards = zeroCost + oneCost + twoCost + threeCost;
            var averageCost = randomizedCards == 0
                ? 0m
                : (oneCost + (2m * twoCost) + (3m * threeCost)) / randomizedCards;

            var baseStats = FormatDefault(DefaultCounters, counters, historyMode, bannerNote);
            return $"{baseStats}\nAverage Cost: {averageCost.ToString("F2", CultureInfo.CurrentCulture)}";
        }
    }
}
