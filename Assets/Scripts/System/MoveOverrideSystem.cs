using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MoveOverrideSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
        RefRW<UnitMover> unitMover,
        RefRO<LocalTransform> localTransform,
        RefRO<MoveOverride> moveOverride,
        EnabledRefRW<MoveOverride> moveOverrideEnabled) in
        SystemAPI.Query<
        RefRW<UnitMover>,
        RefRO<LocalTransform>,
        RefRO<MoveOverride>,
        // 检查组件是否启用EnabledRefRW
        EnabledRefRW<MoveOverride>>())
        {
            if (math.distancesq(localTransform.ValueRO.Position, moveOverride.ValueRO.targetPosition)
            > UnitMoverSystem.REACH_TARGET_POSITION_SQ)
            {
                unitMover.ValueRW.targetPosition = moveOverride.ValueRO.targetPosition;
            }
            else
            {
                moveOverrideEnabled.ValueRW = false;
            }
        }
    }
}
