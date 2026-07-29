using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 实体缓冲区创建
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().
        CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRW<LocalTransform> localTransform, RefRO<Bullet> bullet, RefRO<Target> target, Entity entity)
        in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>, RefRO<Target>>().WithEntityAccess())
        {
            Entity targetEntity = target.ValueRO.targetEntity;

            if (targetEntity == Entity.Null)
            {
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(targetEntity);
            ShootVictim targetShootVictim = SystemAPI.GetComponent<ShootVictim>(targetEntity);
            float3 targetPosition = targetLocalTransform.TransformPoint(targetShootVictim.HitLocalPosition);

            // 高速距离判定检测，记录上一帧与这一帧中与目标的距离，若后者大于前者，则纠正
            float distanceBeforeSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);
            float3 direction = targetPosition - localTransform.ValueRW.Position;
            direction = math.normalize(direction);

            localTransform.ValueRW.Position += direction * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            if (distanceAfterSq > distanceBeforeSq)
            {
                localTransform.ValueRW.Position = targetPosition;
            }

            float destroyDistanceSq = 0.002f;
            if (math.distancesq(localTransform.ValueRO.Position, targetPosition) <= destroyDistanceSq)
            {
                RefRW<Health> health = SystemAPI.GetComponentRW<Health>(targetEntity);
                health.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                health.ValueRW.onHealthChanged = true;

                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
}
