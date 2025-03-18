using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.Pudge;

[Serializable, NetSerializable]
[ByRefEvent] public sealed partial class PudgeDismemberDoAfterEvent : SimpleDoAfterEvent { }
