using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

partial struct ZombieSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // 如果EntitesReferences不存在，则不执行OnUpdate方法
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);



        EntityCommandBuffer entityCommandBuffer =
        SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRW<ZombieSpawner> zombieSpawner, RefRW<LocalTransform> localTransform)
        in SystemAPI.Query<RefRW<ZombieSpawner>, RefRW<LocalTransform>>())
        {
            zombieSpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (zombieSpawner.ValueRO.timer > 0f) continue;

            zombieSpawner.ValueRW.timer = zombieSpawner.ValueRO.timerMax;

            distanceHitList.Clear();
            // 创建过滤器
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNIT_LAYER,
                GroupIndex = 0,
            };

            int nearbyZombieAmount = 0;
            if (collisionWorld.OverlapSphere(
                localTransform.ValueRO.Position,
                zombieSpawner.ValueRO.nearbyZombieAmountDistance,
                ref distanceHitList,
                collisionFilter))
            {
                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    if (!SystemAPI.Exists(distanceHit.Entity))
                    {
                        continue;
                    }
                    if (SystemAPI.HasComponent<Unit>(distanceHit.Entity) && SystemAPI.HasComponent<Zombie>(distanceHit.Entity))
                    {
                        nearbyZombieAmount++;
                    }
                }
            }

            if (nearbyZombieAmount >= zombieSpawner.ValueRO.nearbyZombieAmountMax)
            {
                continue;
            }


            Entity zombieEntity = state.EntityManager.Instantiate(entitiesReferences.zombiePrefabEntity);
            SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

            entityCommandBuffer.AddComponent(zombieEntity, new RandomWalking()
            {
                originPosition = localTransform.ValueRO.Position,
                targetPosition = localTransform.ValueRO.Position,
                distanceMin = zombieSpawner.ValueRO.randomWalkingDistanceMin,
                distanceMax = zombieSpawner.ValueRO.randomWalkingDistanceMax,
                random = new Unity.Mathematics.Random((uint)zombieEntity.Index),
            });
        }
    }
}
