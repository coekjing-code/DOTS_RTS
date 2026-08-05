using Unity.Entities;
using UnityEngine;

class AnimatedMeshAuthoring : MonoBehaviour
{
    public GameObject meshGameObject;
    class AnimatedMeshAuthoringBaker : Baker<AnimatedMeshAuthoring>
    {
        public override void Bake(AnimatedMeshAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimatedMesh()
            {
                meshEntity = GetEntity(authoring.meshGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct AnimatedMesh : IComponentData
{
    public Entity meshEntity;
}