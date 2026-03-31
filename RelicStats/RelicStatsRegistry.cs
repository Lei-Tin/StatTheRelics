using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatTheRelics.RelicStats {
    public static class RelicStatsRegistry {
        static readonly ConcurrentDictionary<string, BaseRelicStats> registry = new();
        static readonly IReadOnlyList<string> defaultCounters = new [] { "Flashes" };

        public static void RegisterAllFromAssembly(Assembly asm) {
            try {
                var defs = asm.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(BaseRelicStats).IsAssignableFrom(t))
                    .Select(t => Activator.CreateInstance(t) as BaseRelicStats)
                    .Where(d => d != null && !string.IsNullOrEmpty(d.TypeName));
                foreach (var def in defs) {
                    registry[def!.TypeName] = def;
                }
            } catch { }
        }

        public static BaseRelicStats? GetDefinition(string? typeName) {
            if (typeName != null && registry.TryGetValue(typeName, out var def)) return def;
            return null;
        }

        public static IReadOnlyList<string> GetDefaultCounters(string? typeName) {
            if (typeName != null && registry.TryGetValue(typeName, out var def)) return def.DefaultCounters;
            return defaultCounters;
        }

        public static IReadOnlyList<string> DefaultCounters => defaultCounters;
    }
}
