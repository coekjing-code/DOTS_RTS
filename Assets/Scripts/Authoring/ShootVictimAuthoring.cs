using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class ShootVictimAuthoring : MonoBehaviour
{
    public Transform HitPositionTransform;
    class ShootVictimAuthoringBaker : Baker<ShootVictimAuthoring>
    {
        public override void Bake(ShootVictimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim()
            {
                HitLocalPosition = authoring.HitPositionTransform.localPosition,
            });
        }
    }
}

public struct ShootVictim : IComponentData
{
    public float3 HitLocalPosition;
}

