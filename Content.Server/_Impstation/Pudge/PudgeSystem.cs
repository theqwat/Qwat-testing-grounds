using Content.Shared._Impstation.Pudge;
using Content.Server.Popups;
using Robust.Server.Audio;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server._Impstation.Pudge;

public sealed partial class PudgeSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public EntProtoId MeatHookPrototype = "FishingRodPudge";
    public EntProtoId MeatShieldPrototype = "ShieldPudge";
}
