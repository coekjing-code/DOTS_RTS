using Unity.Burst;
using Unity.Entities;

[UpdateAfter(typeof(ShootAttackSystem))]
partial struct AnimationStateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRO<AnimatedMesh> animatedMesh, RefRO<UnitMover> unitMover, RefRO<UnitAnimations> unitAnimations)
            in SystemAPI.Query<RefRO<AnimatedMesh>, RefRO<UnitMover>, RefRO<UnitAnimations>>())
        {
            RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
            if (unitMover.ValueRO.isMoving)
            {
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.walkAnimationType;
            }
            else
            {
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.idleAnimationType;
            }
        }

        foreach ((RefRO<AnimatedMesh> animatedMesh, RefRO<ShootAttack> shootAttack,
            RefRO<UnitAnimations> unitAnimations, RefRO<UnitMover> unitMover, RefRO<Target> target)
            in SystemAPI.Query<RefRO<AnimatedMesh>, RefRO<ShootAttack>, RefRO<UnitAnimations>, RefRO<UnitMover>, RefRO<Target>>())
        {
            if (!unitMover.ValueRO.isMoving && target.ValueRO.targetEntity != Entity.Null)
            {
                RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
                activeAnimation.ValueRW.activeAnimationType = AnimationDataSO.AnimationType.SoldierAim;
            }

            if (shootAttack.ValueRO.onShoot.isTriggered)
            {
                RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.shootAnimationType;
            }
        }
    }
}
