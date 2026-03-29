using System.Collections.Generic;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class GamePieceStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.GamePiece";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;
    }
}