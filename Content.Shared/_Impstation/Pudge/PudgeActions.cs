using Content.Shared.Actions;

namespace Content.Shared._Impstation.Pudge;

public sealed partial class PudgeToggleMeatHookEvent : InstantActionEvent { }
public sealed partial class PudgeRotEvent : InstantActionEvent { }
public sealed partial class PudgeMeatShieldEvent : InstantActionEvent { }
public sealed partial class PudgeMeatShieldBreakEvent : InstantActionEvent { }
public sealed partial class PudgeDismemberEvent : EntityTargetActionEvent { }

[RegisterComponent]
public sealed partial class MeatShieldComponent : Component { }
[RegisterComponent]
public sealed partial class MeatHookComponent : Component { }
