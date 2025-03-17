using Content.Shared._Impstation.Pudge;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.Pudge;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]

public sealed partial class PudgeComponent : PudgeComponent
{
    [DataField("MeatHookSFX")]
    public SoundSpecifier MeatHookSFX = new SoundPathSpecifier("/Audio/_Impstation/CosmicCult/ability_imposition.ogg")
}
