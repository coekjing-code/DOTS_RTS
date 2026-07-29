using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class RandomWalkingAuthoring : MonoBehaviour
{
    public float3 originPosition;
    public float3 targetPosition;
    public float distanceMin;
    public float minDistance;
    public uint randomSeed;
    class RandomWalkingBaker : Baker<RandomWalkingAuthoring>
    {
        public override void Bake(RandomWalkingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RandomWalking()
            {
                originPosition = authoring.originPosition,
                targetPosition = authoring.targetPosition,
                distanceMin = authoring.distanceMin,
                distanceMax = authoring.minDistance,
                random = new Unity.Mathematics.Random(authoring.randomSeed),
            });
        }
    }
}

public struct RandomWalking : IComponentData
{
    public float3 originPosition;
    public float3 targetPosition;
    public float distanceMin;
    public float distanceMax;
    public Unity.Mathematics.Random random;
}

