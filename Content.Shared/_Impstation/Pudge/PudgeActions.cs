using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.Pudge;

[ByRefEvent] public sealed partial class PudgeToggleMeatHookEvent : InstantActionEvent { }
[ByRefEvent] public sealed partial class PudgeRotEvent : InstantActionEvent { }
[ByRefEvent] public sealed partial class PudgeToggleMeatShieldEvent : InstantActionEvent { }
[ByRefEvent] public sealed partial class PudgeDismemberEvent : EntityTargetActionEvent { }
//using ByRefEvent this many times is scary.
