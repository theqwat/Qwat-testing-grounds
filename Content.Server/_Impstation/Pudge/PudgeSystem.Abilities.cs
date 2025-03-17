using Content.Shared._Impstation.Pudge;
using Robust.Shared.Prototypes;

namespace Content.Server._Impstation.Pudge;

public sealed partial class PudgeSystem : EntitySystem
{
    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<PudgeComponent, PudgeToggleMeatHookEvent>(OnMeatHook);
        SubscribeLocalEvent<PudgeComponent, PudgeRotEvent>(OnRot);
        SubscribeLocalEvent<PudgeComponent, PudgeToggleMeatShieldEvent>(OnMeatShield);
        SubscribeLocalEvent<PudgeComponent, PudgeDismemberEvent>(OnDismember);
    }

    private void OnMeatHook(EntityUid uid, PudgeComponent comp, ref PudgeToggleMeatHookEvent args)
    {
        if (!TryToggleItem(uid, MeatHookPrototype, comp))
            return;
        _audio.PlayPvs(uid.Comp.MeatHookSFX, uid);
    }

    private void OnRot()
    {
        return;
    }

    private void OnMeatShield(EntityUid uid, PudgeComponent comp, ref PudgeToggleMeatShieldEvent args)
    {
        if (!TryToggleItem(uid, MeatShieldPrototype, comp))
            return;
        _audio.PlayPvs(uid.Comp.MeatShieldSFX, uid);
    }

    private void OnDismember()
    {
        return;
    }

    public bool TryToggleItem(EntityUid uid, EntProtoId proto, PudgeComponent component)
    {
        if (!component.Equipment.TryGetValue(proto.Id, out var item))
        {
            item = Spawn(proto, Transform(uid).Coordinates);
            if (!_hands.TryForcePickupAnyHand(uid, (EntityUid) item))
            {
                _popup.PopupEntity(Loc.GetString("pudge-fail=hands"), uid, uid);
                QueueDel(item);
                return false;
            }
            component.Equipment.Add(proto.Id, item);
            return true;
        }

        QueueDel(item);
        component.Equipment.Remove(proto.Id);
        return true;
    }
}
