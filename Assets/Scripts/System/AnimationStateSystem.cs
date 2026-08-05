using Unity.Burst;
using Unity.Entities;

partial struct AnimationStateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRO<AnimatedMesh> animatedMesh, RefRW<UnitMover> unitMover, RefRO<UnitAnimations> unitAnimations) 
            in SystemAPI.Query<RefRO<AnimatedMesh>, RefRW<UnitMover>, RefRO<UnitAnimations>>())
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
    }
}
