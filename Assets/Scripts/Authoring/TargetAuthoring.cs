using Unity.Entities;
using UnityEngine;

class TargetAuthoring : MonoBehaviour
{
    public Entity targetEntity;
    class TargetAuthoringBaker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Target()
            {
                targetEntity = authoring.targetEntity,
            });
        }
    }
}

public struct Target : IComponentData
{
    public Entity targetEntity;
}
