using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatTheRelics.RelicStats {
    // Base definition for per-relic stat configuration. Override DefaultCounters and/or Format for custom displays.
    public abstract class BaseRelicStats {
        protected static readonly IReadOnlyList<string> DefaultFlashes = new [] { "Flashes" };
        public abstract string TypeName { get; }
        public virtual IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public virtual string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            return FormatDefault(DefaultCounters, counters, historyMode, bannerNote);
        }

        public static string FormatDefault(IReadOnlyList<string> desiredKeys, IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            var keys = desiredKeys ?? DefaultFlashes;
            var data = counters ?? new Dictionary<string,int>();

            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);

            if (keys != null) {
                foreach (var key in keys) {
                    var val = data.TryGetValue(key, out var v) ? v : 0;
                    sb.AppendLine($"{key}: {val}");
                }
            }

            foreach (var kv in data.OrderBy(k => k.Key)) {
                if (keys != null && keys.Contains(kv.Key)) continue;
                sb.AppendLine($"{kv.Key}: {kv.Value}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
