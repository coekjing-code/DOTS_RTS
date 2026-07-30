using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// 保证Unit在场景编辑位置生成，而不是(0, 0, 0)
/// </summary>
partial struct SetupUnitMoverDefaultPositionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton
        <EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach ((
            RefRW<LocalTransform> localTransform,
            RefRW<UnitMover> unitMover,
            RefRO<SetupUnitMoverDefaultPosition> setupUnitMoverDefaultPosition,
            Entity entity)
            in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<UnitMover>,
                RefRO<SetupUnitMoverDefaultPosition>>().WithEntityAccess())
        {
            unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            entityCommandBuffer.RemoveComponent<SetupUnitMoverDefaultPosition>(entity);
        }
    }
}
