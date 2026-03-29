using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class StorybookStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Storybook";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}