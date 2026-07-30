using Unity.Entities;
using UnityEngine;

class LoseTargetAuthoring : MonoBehaviour
{
    public float loseTargetDistance;
    class LoseTargetAuthoringBaker : Baker<LoseTargetAuthoring>
    {
        public override void Bake(LoseTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LoseTarget()
            {
                loseTargetDistance = authoring.loseTargetDistance,
            });
        }
    }
}

public struct LoseTarget : IComponentData
{
    public float loseTargetDistance;
}
