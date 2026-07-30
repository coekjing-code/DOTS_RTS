using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct LoseTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
        RefRO<LocalTransform> localTransform,
        RefRW<Target> target,
        RefRO<LoseTarget> loseTarget,
        RefRO<TargetOverride> targetOverride)
        in SystemAPI.Query<
        RefRO<LocalTransform>,
        RefRW<Target>,
        RefRO<LoseTarget>,
        RefRO<TargetOverride>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;

            if (targetOverride.ValueRO.targetEntity == Entity.Null) continue;

            LocalTransform targetLocalTranform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            float currentTargetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTranform.Position);
            if (currentTargetDistance > loseTarget.ValueRO.loseTargetDistance)
            {
                target.ValueRW.targetEntity = Entity.Null;
            }
        }
    }
}
