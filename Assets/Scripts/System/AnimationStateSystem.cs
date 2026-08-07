using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(ShootAttackSystem))]
partial struct AnimationStateSystem : ISystem
{
    private ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        activeAnimationComponentLookup = state.GetComponentLookup<ActiveAnimation>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        activeAnimationComponentLookup.Update(ref state);

        IdleWalkingAnimationStateJob idleWalkingAnimationStateJob = new IdleWalkingAnimationStateJob
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup,
        };
        idleWalkingAnimationStateJob.ScheduleParallel();
        // foreach ((RefRO<AnimatedMesh> animatedMesh, RefRO<UnitMover> unitMover, RefRO<UnitAnimations> unitAnimations)
        //     in SystemAPI.Query<RefRO<AnimatedMesh>, RefRO<UnitMover>, RefRO<UnitAnimations>>())
        // {
        //     RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
        //     if (unitMover.ValueRO.isMoving)
        //     {
        //         activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.walkAnimationType;
        //     }
        //     else
        //     {
        //         activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.idleAnimationType;
        //     }
        // }

        AimShootAnimationStateJob shootAttackAnimationStateJob = new AimShootAnimationStateJob
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup,
        };
        shootAttackAnimationStateJob.ScheduleParallel();

        // foreach ((RefRO<AnimatedMesh> animatedMesh, RefRO<ShootAttack> shootAttack,
        //     RefRO<UnitAnimations> unitAnimations, RefRO<UnitMover> unitMover, RefRO<Target> target)
        //     in SystemAPI.Query<RefRO<AnimatedMesh>, RefRO<ShootAttack>, RefRO<UnitAnimations>, RefRO<UnitMover>, RefRO<Target>>())
        // {
        //     if (!unitMover.ValueRO.isMoving && target.ValueRO.targetEntity != Entity.Null)
        //     {
        //         RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
        //         activeAnimation.ValueRW.activeAnimationType = unitAnimations.ValueRO.aimAnimationType;
        //     }

        //     if (shootAttack.ValueRO.onShoot.isTriggered)
        //     {
        //         RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
        //         activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.shootAnimationType;
        //     }
        // }

        MeleeAttackAnimationStateJob meleeAttackAnimationStateJob = new MeleeAttackAnimationStateJob
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup,
        };
        meleeAttackAnimationStateJob.ScheduleParallel();

        // foreach ((RefRO<AnimatedMesh> animatedMesh, RefRO<MeleeAttack> meleeAttack,
        //     RefRO<UnitAnimations> unitAnimations)
        //     in SystemAPI.Query<RefRO<AnimatedMesh>, RefRO<MeleeAttack>, RefRO<UnitAnimations>>())
        // {
        //     if (meleeAttack.ValueRO.onAttacked)
        //     {
        //         RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
        //         activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.meleeAnimationType;
        //     }
        // }
    }
}

public partial struct IdleWalkingAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;
    public void Execute(in AnimatedMesh animatedMesh, in UnitMover unitMover, in UnitAnimations unitAnimations)
    {
        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);
        if (unitMover.isMoving)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.walkAnimationType;
        }
        else
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.idleAnimationType;
        }
    }
}

public partial struct AimShootAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;
    public void Execute(in AnimatedMesh animatedMesh, in UnitMover unitMover, in Target target, in ShootAttack shootAttack, in UnitAnimations unitAnimations)
    {
        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);

        if (!unitMover.isMoving && target.targetEntity != Entity.Null)
        {
            activeAnimation.ValueRW.activeAnimationType = unitAnimations.aimAnimationType;
        }
        if (shootAttack.onShoot.isTriggered)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.shootAnimationType;
        }
    }
}

public partial struct MeleeAttackAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;
    public void Execute(in AnimatedMesh animatedMesh, in MeleeAttack meleeAttack, in UnitAnimations unitAnimations)
    {
        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);

        if (meleeAttack.onAttacked)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.meleeAnimationType;
        }
    }
}