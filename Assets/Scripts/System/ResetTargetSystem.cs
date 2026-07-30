using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct ResetTargetSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformComponentLookup;
    private EntityStorageInfoLookup entityStorageInfoLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(true);
        entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        localTransformComponentLookup.Update(ref state);
        entityStorageInfoLookup.Update(ref state);
        ResetTargetJob resetTargetJob = new ResetTargetJob()
        {
            localTransformComponentLookup = localTransformComponentLookup,
            entityStorageInfoLookup = entityStorageInfoLookup,
        };
        resetTargetJob.ScheduleParallel();

        ResetTargetOverrideJob resetTargetOverrideJob = new ResetTargetOverrideJob()
        {
            localTransformComponentLookup = localTransformComponentLookup,
            entityStorageInfoLookup = entityStorageInfoLookup,
        };
        resetTargetOverrideJob.ScheduleParallel();
        // foreach (RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        // {
        //     if (target.ValueRO.targetEntity != Entity.Null)
        //     {
        //         if (!SystemAPI.Exists(target.ValueRW.targetEntity) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRW.targetEntity))
        //         {
        //             target.ValueRW.targetEntity = Entity.Null;
        //         }
        //     }
        // }

        // foreach (RefRW<TargetOverride> targetOverride in SystemAPI.Query<RefRW<TargetOverride>>())
        // {
        //     if (targetOverride.ValueRO.targetEntity != Entity.Null)
        //     {
        //         if (!SystemAPI.Exists(targetOverride.ValueRW.targetEntity) || !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRW.targetEntity))
        //         {
        //             targetOverride.ValueRW.targetEntity = Entity.Null;
        //         }
        //     }
        // }
    }
}

public partial struct ResetTargetJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    public void Execute(ref Target target)
    {
        if (target.targetEntity != Entity.Null)
        {
            if (!entityStorageInfoLookup.Exists(target.targetEntity) || !localTransformComponentLookup.HasComponent(target.targetEntity))
            {
                target.targetEntity = Entity.Null;
            }
        }
    }
}

public partial struct ResetTargetOverrideJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;

    public void Execute(ref TargetOverride targetOverride)
    {
                    if (targetOverride.targetEntity != Entity.Null)
            {
                if (!entityStorageInfoLookup.Exists(targetOverride.targetEntity) || !localTransformComponentLookup.HasComponent(targetOverride.targetEntity))
                {
                    targetOverride.targetEntity = Entity.Null;
                }
            }
    }
}
