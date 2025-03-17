using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared._Impstation.Pudge;

[RegisterComponent, NetworkedComponent]
public sealed partial class PudgeComponent : Component
{
    [DataField] public SoundSpecifier MeatHookSFX = new SoundPathSpecifier("/Audio/_Impstation/CosmicCult/ability_imposition.ogg");
    [DataField] public SoundSpecifier MeatShieldSFX = new SoundPathSpecifier("/Audio/_Impstation/CosmicCult/ability_imposition.ogg");
}
public sealed partial class PudgeToggleMeatHookEvent : InstantActionEvent { }
public sealed partial class PudgeRotEvent : InstantActionEvent { }
public sealed partial class PudgeToggleMeatShieldEvent : InstantActionEvent { }
public sealed partial class PudgeDismemberEvent : EntityTargetActionEvent { }
