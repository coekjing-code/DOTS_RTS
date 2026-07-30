using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootLightSpawnerSystem : ISystem
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

        foreach (RefRO<ShootAttack> shootLight in SystemAPI.Query<RefRO<ShootAttack>>())
        {
            Entity shootLightEntity = state.EntityManager.Instantiate(entitiesReferences.shootLightPrefabEntity);
            SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shootLight.ValueRO.onShoot.shootFromPosition));
        }
    }
}
