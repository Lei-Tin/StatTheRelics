using System;
using System.Reflection;
using HarmonyLib;

namespace StatTheRelics.Patches {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class PatchTargetAlternativeAttribute : Attribute {
        public PatchTargetAlternativeAttribute(Type targetType, string methodName, params Type[] argumentTypes) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class VersionOptionalPatchAttribute : Attribute { }

    internal readonly struct PatchTargetCandidate {
        public string MethodName { get; }
        public Type[]? ArgumentTypes { get; }

        public PatchTargetCandidate(string methodName) {
            MethodName = methodName;
            ArgumentTypes = null;
        }

        public PatchTargetCandidate(string methodName, params Type[] argumentTypes) {
            MethodName = methodName;
            ArgumentTypes = argumentTypes;
        }
    }

    internal static class PatchTargetResolver {
        public static MethodBase RequireAny(Type targetType, params PatchTargetCandidate[] candidates) {
            foreach (var candidate in candidates) {
                MethodBase? method;
                if (candidate.ArgumentTypes == null) {
                    var namedMethods = AccessTools.GetDeclaredMethods(targetType)
                        .FindAll(candidateMethod => candidateMethod.Name == candidate.MethodName);
                    method = namedMethods.Count == 1
                        ? namedMethods[0]
                        : namedMethods.Count == 0
                            ? null
                            : throw new AmbiguousMatchException($"Multiple overloads found for {targetType.FullName}.{candidate.MethodName}; declare argument types.");
                } else {
                    method = AccessTools.DeclaredMethod(targetType, candidate.MethodName, candidate.ArgumentTypes);
                }
                if (method != null) return method;
            }

            var signatures = string.Join(" or ", Array.ConvertAll(candidates, candidate =>
                candidate.ArgumentTypes == null
                    ? $"{targetType.FullName}.{candidate.MethodName}"
                    : $"{targetType.FullName}.{candidate.MethodName}({string.Join(", ", Array.ConvertAll(candidate.ArgumentTypes, type => type.FullName))})"));
            throw new MissingMethodException($"None of the required patch targets were found: {signatures}");
        }
    }
}
