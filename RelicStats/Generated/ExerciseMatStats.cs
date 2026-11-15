using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ExerciseMatStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.ExerciseMat";
        public override IReadOnlyList<string> DefaultCounters => new[] { "Stretch Gained" };
    }
}
