using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> localTransform, RefRW<UnitMover> unitMover, RefRW<RandomWalking> randomWalking)
        in SystemAPI.Query<RefRW<LocalTransform>, RefRW<UnitMover>, RefRW<RandomWalking>>())
        {
            if (math.distancesq(localTransform.ValueRO.Position, randomWalking.ValueRO.targetPosition) <
            UnitMoverSystem.REACH_TARGET_POSITION_SQ)
            {
                // 获取random实例化
                Random random = randomWalking.ValueRO.random;

                // 抵达位置后更新randomWalking的targetPosition
                float3 randomDirection = new float3(random.NextFloat(-1f, 1f), 0, random.NextFloat(-1f, 1f));
                randomDirection = math.normalize(randomDirection);
                randomWalking.ValueRW.targetPosition = randomWalking.ValueRO.originPosition
                + randomDirection * random.NextFloat(randomWalking.ValueRO.distanceMin, randomWalking.ValueRO.distanceMax);
                // 使用后保存
                randomWalking.ValueRW.random = random;
            }
            else
            {
                // 设置unitMover的targetPosition
                unitMover.ValueRW.targetPosition = randomWalking.ValueRO.targetPosition;
            }
        }
    }
}
