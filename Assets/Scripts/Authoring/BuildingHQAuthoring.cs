using Unity.Entities;
using UnityEngine;

class BuildingHQAuthoring : MonoBehaviour
{
    class BuildingHQAuthoringBaker : Baker<BuildingHQAuthoring>
    {
        public override void Bake(BuildingHQAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingHQ());        
        }
    }
}

public struct BuildingHQ : IComponentData
{
    
}
