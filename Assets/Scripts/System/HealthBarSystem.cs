using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// 确保HealthBarSystem在ResetEventsSystem之前调用
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // dots系统的执行会早于unity引擎系统，因此此处要做安全检查
        Vector3 cameraForward = Vector3.zero;
        if (Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }

        foreach ((RefRW<HealthBar> healthBar, RefRW<LocalTransform> localTransform)
        in SystemAPI.Query<RefRW<HealthBar>, RefRW<LocalTransform>>())
        {
            // 基于父物体做一个面向相机前方的旋转
            LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
            if (localTransform.ValueRO.Scale == 1f)
            {
                localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
            }

            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

            if (!health.onHealthChanged)
            {
                continue;
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
                SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);

            // RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(healthBar.ValueRO.barVisualEntity);
            // localTransform.ValueRW.Scale = healthNormalized;
        }
    }
}
