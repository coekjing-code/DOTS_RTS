using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// 确保HealthBarSystem在ResetEventsSystem之前调用
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    [ReadOnly] public ComponentLookup<Health> healthComponentLookup;
    // 第一个特性表示多个Jobs可以同时安全地访问同一原生容器
    [NativeDisableParallelForRestriction] [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [NativeDisableParallelForRestriction] [ReadOnly] public ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>();
        healthComponentLookup = state.GetComponentLookup<Health>(true);
        postTransformMatrixComponentLookup = state.GetComponentLookup<PostTransformMatrix>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // dots系统的执行会早于unity引擎系统，因此此处要做安全检查
        Vector3 cameraForward = Vector3.zero;
        if (Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }

        localTransformComponentLookup.Update(ref state);
        healthComponentLookup.Update(ref state);
        postTransformMatrixComponentLookup.Update(ref state);
        HealthBarJob healthBarJob = new HealthBarJob()
        {
            cameraForward = cameraForward,
            localTransformComponentLookup = localTransformComponentLookup,
            healthComponentLookup = healthComponentLookup,
            postTransformMatrixComponentLookup = postTransformMatrixComponentLookup,
        };
        healthBarJob.ScheduleParallel();
        // 原代码
    }
}

public partial struct HealthBarJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public ComponentLookup<Health> healthComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;
    public float3 cameraForward;

    public void Execute(in HealthBar healthBar, Entity entity)
    {
        // 基于父物体做一个面向相机前方的旋转
        RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);
        LocalTransform parentLocalTransform = localTransformComponentLookup[healthBar.healthEntity];
        if (localTransform.ValueRO.Scale == 1f)
        {
            localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
        }
        Health health = healthComponentLookup[healthBar.healthEntity];
        if (!health.onHealthChanged)
        {
            return;
        }
        float healthNormalized = (float)health.healthAmount / health.healthAmountMax;
        if (healthNormalized == 1f)
        {
            localTransform.ValueRW.Scale = 0f;
        }
        else
        {
            localTransform.ValueRW.Scale = 1f;
        }
        // 当Entity的Scale不是(1, 1, 1)时，会自动添加PostTransformMatrix组件
        // 或者将TransformUsageFlags设为NonUniformScale也会添加
        RefRW<PostTransformMatrix> barVisualPostTransformMatrix =
            postTransformMatrixComponentLookup.GetRefRW(healthBar.barVisualEntity);
        barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
    }
}
