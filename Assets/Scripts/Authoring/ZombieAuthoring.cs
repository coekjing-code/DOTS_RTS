using Unity.Entities;
using UnityEngine;

class ZombieAuthoring : MonoBehaviour
{
    class ZombieAuthoringBaker : Baker<ZombieAuthoring>
    {
        public override void Bake(ZombieAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Zombie());        
        }
    }
}

public struct Zombie : IComponentData
{
    
}