using Content.Shared.Actions;
using Content.Shared.DoAfter;

namespace Content.Shared._Impstation.Pudge;

public sealed partial class PudgeToggleMeatHookEvent : InstantActionEvent { }
public sealed partial class PudgeRotEvent : InstantActionEvent { }
public sealed partial class PudgeToggleMeatShieldEvent : InstantActionEvent { }
public sealed partial class PudgeDismemberEvent : EntityTargetActionEvent { }
public sealed partial class PudgeDismemberDoAfterEvent : SimpleDoAfterEvent { }
public sealed partial class Wagabagabobo : InstantActionEvent { }
