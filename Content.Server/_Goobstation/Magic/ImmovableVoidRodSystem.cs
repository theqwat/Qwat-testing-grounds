using Content.Server.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Maps;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Map.Components;
using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.Polymorph.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server.Magic;

public sealed partial class ImmovableVoidRodSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we are deliberately including paused entities. rod hungers for all
        foreach (var (rod, trans) in EntityManager.EntityQuery<ImmovableVoidRodComponent, TransformComponent>(true))
        {
            rod.Accumulator += frameTime;

            if (rod.Accumulator > rod.Lifetime.TotalSeconds)
            {
                QueueDel(rod.Owner);
                return;
            }

            if (!_ent.TryGetComponent<MapGridComponent>(trans.GridUid, out var grid))
                continue;



            var tileref = grid.GetTileRef(trans.Coordinates);
            var tile = _prot.Index<ContentTileDefinition>("FloorAstroSnow");
            _tile.ReplaceTile(tileref, tile);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ImmovableVoidRodComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<ImmovableVoidRodComponent> ent, ref StartCollideEvent args)
    {
        var impEnt = args.OtherEntity;

        if ((TryComp<HereticComponent>(args.OtherEntity, out var th) && th.CurrentPath == "Void")
        || HasComp<GhoulComponent>(args.OtherEntity))
            return;

        _stun.TryParalyze(args.OtherEntity, TimeSpan.FromSeconds(2.5f), false);

        TryComp<TagComponent>(args.OtherEntity, out var tag);
        var tags = tag?.Tags ?? new();
/*
        if (TryComp<BodyComponent>(impEnt, out var body))
        {
            if (!impEnt.ShouldGib)
            {
                if (args.Damage == null)
                    return;

                _damageable.TryChangeDamage(ent, component.Damage, ignoreResistances: true);
                return;
            }

            _bodySystem.GibBody(ent, body: body);
            return;
        }
*/
        if (tags.Contains("Wall") && Prototype(args.OtherEntity) != null && Prototype(args.OtherEntity)!.ID != "WallSnowCobblebrick")
        {
            Spawn("WallSnowCobblebrick", Transform(args.OtherEntity).Coordinates);
            QueueDel(args.OtherEntity);
        }
    }
}