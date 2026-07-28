using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct TestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        int unitCount = 0;

        foreach (RefRO<Friendly> friendlyComponent in SystemAPI.Query<RefRO<Friendly>>())
        {
            unitCount++;
        }

        // Debug.Log("当前士兵数量 : " + unitCount);
    }
}