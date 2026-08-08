using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatTheRelics.RelicStats {
    public static class RelicStatsRegistry {
        static readonly ConcurrentDictionary<string, BaseRelicStats> registry = new();
        static readonly ConcurrentDictionary<string, byte> implementationChanged = new();
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
            if (IsImplementationChanged(typeName)) return defaultCounters;
            if (typeName != null && registry.TryGetValue(typeName, out var def)) return def.DefaultCounters;
            return defaultCounters;
        }

        public static void MarkImplementationChanged(string typeName) {
            if (!string.IsNullOrWhiteSpace(typeName)) implementationChanged[typeName] = 0;
        }

        public static bool IsImplementationChanged(string? typeName) {
            return typeName != null && implementationChanged.ContainsKey(typeName);
        }

        public static string? ResolveRelicTypeNameForPatchClass(Type patchType) {
            if (patchType == null) return null;

            return registry.Keys
                .Select(typeName => new {
                    TypeName = typeName,
                    SimpleName = typeName[(typeName.LastIndexOf('.') + 1)..]
                })
                .Where(candidate => patchType.Name.StartsWith(candidate.SimpleName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(candidate => candidate.SimpleName.Length)
                .Select(candidate => candidate.TypeName)
                .FirstOrDefault();
        }

        public static IReadOnlyList<string> DefaultCounters => defaultCounters;
    }
}
