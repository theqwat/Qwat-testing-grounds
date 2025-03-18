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
using Content.Shared.Hands.Components;
using Timer = Robust.Shared.Timing.Timer;
using Content.Shared.Hands.EntitySystems;

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
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    private readonly SoundSpecifier _meatHookSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _rotSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _meatShieldSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _deflectSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _dismemberSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly SoundSpecifier _chowSFX = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
    private readonly EntProtoId _meatShieldVFX = "PudgeMeatShieldVFX";
    public ProtoId<DamageGroupPrototype> ChowDamageGroup = "Brute";
    public override void Initialize()
    {
        base.Initialize();
        //NO PUDGE COMPONENT!!!!!
        SubscribeLocalEvent<HandsComponent, PudgeToggleMeatHookEvent>(OnMeatHook);
        SubscribeLocalEvent<HandsComponent, PudgeRotEvent>(OnRot);
        SubscribeLocalEvent<HandsComponent, PudgeMeatShieldEvent>(OnMeatShield);
        SubscribeLocalEvent<MeatShieldComponent, BeforeDamageChangedEvent>(OnMeatShieldDamaged);
        SubscribeLocalEvent<HandsComponent, PudgeDismemberEvent>(OnDismember);
        SubscribeLocalEvent<HandsComponent, PudgeDismemberDoAfterEvent>(OnDismemberDoAfter);
    }

    private void OnMeatHook(EntityUid uid, HandsComponent component, ref PudgeToggleMeatHookEvent args)
    {
        if (!TryToggleItem(args.Performer, "MeatHookPudge", component))
            return;
        _audio.PlayPvs(_meatHookSFX, args.Performer, AudioParams.Default.WithVolume(-3f));
    }

    public bool TryToggleItem(EntityUid uid, EntProtoId proto, HandsComponent component)
    {
        if (HasComp<MeatHookComponent>(uid))
            return false;
        var item = Spawn(proto, Transform(uid).Coordinates);
        if (!_hands.TryForcePickupAnyHand(uid, item))
        {
            _popup.PopupEntity(Loc.GetString("pudge-no-hands"), uid, uid);
            QueueDel(item);
            return false;
        }
        EnsureComp<MeatHookComponent>(uid);
        return true;
    }

    private void OnRot(EntityUid uid, HandsComponent component, ref PudgeRotEvent args)
    {
        _popup.PopupEntity(Loc.GetString("pudge-rot-popup"), args.Performer, args.Performer);
        _audio.PlayPvs(_rotSFX, args.Performer, AudioParams.Default.WithVolume(-3f));

        var tileMix = _atmos.GetTileMixture(args.Performer, excite: true);
        tileMix?.AdjustMoles(Gas.Ammonia, 300);
        //MAYBE ADD A FOAM CLOUD OR PUDDLE SPILL OR SOMETHING
    }

    private void OnMeatShield(EntityUid uid, HandsComponent component, ref PudgeMeatShieldEvent args)
    {
        if (HasComp<MeatShieldComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("pudge-ability-active"), uid, uid);
            return;
        }
        _audio.PlayPvs(_meatShieldSFX, uid, AudioParams.Default.WithVolume(-3f));
        EnsureComp<MeatShieldComponent>(uid, out var shield);
        Timer.Spawn(TimeSpan.FromSeconds(5.8), () => RemComp(uid, shield));
        Spawn(_meatShieldVFX, Transform(uid).Coordinates);
        args.Handled = true;
    }

    private void OnMeatShieldDamaged(Entity<MeatShieldComponent> uid, ref BeforeDamageChangedEvent args)
    {
        args.Cancelled = true;
        _audio.PlayPvs(_deflectSFX, uid, AudioParams.Default.WithVolume(-3f));
    }

    private void OnDismember(EntityUid uid, HandsComponent component, ref PudgeDismemberEvent args)
    {
        var target = args.Target;

        var popupSelf = Loc.GetString("pudge-dismember-start-self", ("target", Identity.Entity(target, EntityManager)));
        var popupTarget = Loc.GetString("pudge-dismember-start-target");
        var popupOthers = Loc.GetString("pudge-dismember-start-others", ("user", Identity.Entity(target, EntityManager)), ("target", Identity.Entity(target, EntityManager)));

        _popup.PopupEntity(popupSelf, uid, uid);
        _popup.PopupEntity(popupTarget, target, target, PopupType.MediumCaution);
        _popup.PopupEntity(popupOthers, uid, Filter.Pvs(uid).RemovePlayersByAttachedEntity([uid, target]), true, PopupType.MediumCaution);

        _audio.PlayPvs(_dismemberSFX, uid, AudioParams.Default.WithVolume(-3f));

        TryDismember(uid, target);
    }
    private void TryDismember(EntityUid uid, EntityUid? target)
    {
        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(0.75), new PudgeDismemberDoAfterEvent(), uid, target)
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
    private void OnDismemberDoAfter(EntityUid uid, HandsComponent component, ref PudgeDismemberDoAfterEvent args)
    {
        var target = args.Args.Target;
        if (args.Cancelled || args.Handled || target == null || _mobState.IsDead(target.Value))
            return;

        _audio.PlayPvs(_chowSFX, uid, AudioParams.Default.WithVolume(-3f));

        var dmg = new DamageSpecifier(_proto.Index(ChowDamageGroup), 11);
        _damageable.TryChangeDamage(target, dmg, false, true);

        var ichorInjection = new Solution("Ichor", 10f);
        _bloodstreamSystem.TryAddToChemicals(uid, ichorInjection);

        TryDismember(uid, target);
    }
}
