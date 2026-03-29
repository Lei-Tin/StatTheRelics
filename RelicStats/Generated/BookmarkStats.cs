using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BookmarkStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Bookmark";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}