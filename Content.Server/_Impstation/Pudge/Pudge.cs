using Content.Shared._Impstation.Pudge;
using Robust.Shared.Prototypes;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Content.Server.Popups;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Server.DoAfter;
using Content.Shared.DoAfter;

namespace Content.Server._Impstation.Pudge;

public sealed partial class PudgeSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    private readonly SoundSpecifier _meatHookSFX = new SoundPathSpecifier("");
    private readonly SoundSpecifier _rotSFX = new SoundPathSpecifier("");
    private readonly SoundSpecifier _meatShieldSFX = new SoundPathSpecifier("");
    private readonly SoundSpecifier _dismemberSFX = new SoundPathSpecifier("");
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PudgeToggleMeatHookEvent>(OnMeatHook);
        SubscribeLocalEvent<PudgeRotEvent>(OnRot);
        SubscribeLocalEvent<PudgeToggleMeatShieldEvent>(OnMeatShield);
        SubscribeLocalEvent<PudgeDismemberEvent>(OnDismember);
        SubscribeLocalEvent<PudgeDismemberDoAfterEvent>(OnDismemberDoAfter);
        SubscribeLocalEvent<Wagabagabobo>(OnDismemberDoAfter);
    }

    private void OnMeatHook(ref PudgeToggleMeatHookEvent args)
    {
        if (!TryToggleItem(args.Performer, "MeatHookPudge"))
            return;
        _audio.PlayPvs(_meatHookSFX, args.Performer);
    }

    private void OnRot(ref PudgeRotEvent args)
    {
        _popup.PopupEntity(Loc.GetString("pudge-rot-popup"), args.Performer, args.Performer);
        _audio.PlayPvs(_rotSFX, args.Performer);

        var tileMix = _atmos.GetTileMixture(args.Performer, excite: true);
        tileMix?.AdjustMoles(Gas.Ammonia, 5);
    }

    private void OnMeatShield(ref PudgeToggleMeatShieldEvent args)
    {
        if (!TryToggleItem(args.Performer, "MeatShieldPudge"))
            return;
        _audio.PlayPvs(_meatShieldSFX, args.Performer);
    }

    private void OnDismember(ref PudgeDismemberEvent args)
    {
        var performer = args.Performer;
        var target = args.Target;

        var popupSelf = Loc.GetString("pudge-dismember-start-self", ("target", Identity.Entity(target, EntityManager)));
        var popupTarget = Loc.GetString("pudge-dismember-start-target");
        var popupOthers = Loc.GetString("pudge-dismember-start-others", ("user", Identity.Entity(target, EntityManager)), ("target", Identity.Entity(target, EntityManager)));

        _popup.PopupEntity(popupSelf, performer, performer);
        _popup.PopupEntity(popupTarget, target, target, PopupType.MediumCaution);
        _popup.PopupEntity(popupOthers, performer, Filter.Pvs(performer).RemovePlayersByAttachedEntity([performer, target]), true, PopupType.MediumCaution);

        _audio.PlayPvs(_dismemberSFX, performer);

        var dargs = new DoAfterArgs(EntityManager, performer, TimeSpan.FromSeconds(2), new (), performer, target)
        {
            DistanceThreshold = 1f,
            BreakOnDamage = false,
            BreakOnHandChange = false,
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    private void OnDismemberDoAfter()
    {}

    public bool TryToggleItem(EntityUid uid, EntProtoId proto)
    {
        return false;
    }
}
