using Unity.Entities;
using UnityEngine;

class TargetAuthoring : MonoBehaviour
{
    public GameObject targetGameObject;
    class TargetAuthoringBaker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Target()
            {
                targetEntity = GetEntity(authoring.targetGameObject, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct Target : IComponentData
{
    public Entity targetEntity;
}
