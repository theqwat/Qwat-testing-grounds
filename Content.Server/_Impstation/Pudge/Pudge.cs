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
using Content.Shared.Mobs.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Chemistry.Components;
using Content.Server.Body.Systems;

namespace Content.Server._Impstation.Pudge;

public sealed partial class PudgeSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    private readonly SoundSpecifier _meatHookSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _rotSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _meatShieldSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _dismemberSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _chowSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    public ProtoId<DamageGroupPrototype> ChowDamageGroup = "Brute";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PudgeToggleMeatHookEvent>(OnMeatHook);
        SubscribeLocalEvent<PudgeRotEvent>(OnRot);
        SubscribeLocalEvent<PudgeToggleMeatShieldEvent>(OnMeatShield);
        SubscribeLocalEvent<PudgeDismemberEvent>(OnDismember);
        SubscribeLocalEvent<PudgeDismemberDoAfterEvent>(OnDismemberDoAfter);
    }

    private void OnMeatHook(ref PudgeToggleMeatHookEvent args)
    {
        if (!TryToggleItem(args.Performer, "MeatHookPudge"))
            return;
        _audio.PlayPvs(_meatHookSFX, args.Performer, AudioParams.Default.WithVolume(-3f));
    }

    private void OnRot(ref PudgeRotEvent args)
    {
        _popup.PopupEntity(Loc.GetString("pudge-rot-popup"), args.Performer, args.Performer);
        _audio.PlayPvs(_rotSFX, args.Performer, AudioParams.Default.WithVolume(-3f));

        var tileMix = _atmos.GetTileMixture(args.Performer, excite: true);
        tileMix?.AdjustMoles(Gas.Ammonia, 30);
    }

    private void OnMeatShield(ref PudgeToggleMeatShieldEvent args)
    {
        if (!TryToggleItem(args.Performer, "MeatShieldPudge"))
            return;
        _audio.PlayPvs(_meatShieldSFX, args.Performer, AudioParams.Default.WithVolume(-3f));
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

        _audio.PlayPvs(_dismemberSFX, performer, AudioParams.Default.WithVolume(-3f));

        TryDismember(performer, target);
    }
    private void TryDismember(EntityUid performer, EntityUid? target)
    {
        var dargs = new DoAfterArgs(EntityManager, performer, TimeSpan.FromSeconds(2), new PudgeDismemberDoAfterEvent(), performer, target)
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
    private void OnDismemberDoAfter(ref PudgeDismemberDoAfterEvent args)
    {
        var target = args.Args.Target;
        if (args.Cancelled || args.Handled || target == null || _mobState.IsDead(target.Value))
            return;

        _audio.PlayPvs(_chowSFX, args.User, AudioParams.Default.WithVolume(-3f));

        var dmg = new DamageSpecifier(_proto.Index(ChowDamageGroup), 11);
        _damageable.TryChangeDamage(target, dmg, false, true);

        var ichorInjection = new Solution("Ichor", 10f);
        _bloodstreamSystem.TryAddToChemicals(args.User, ichorInjection);

        TryDismember(args.User, target);
    }

    public bool TryToggleItem(EntityUid uid, EntProtoId proto)
    {
        return false;
    }
}
