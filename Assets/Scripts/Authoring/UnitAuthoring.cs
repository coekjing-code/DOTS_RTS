using Unity.Entities;
using UnityEngine;

class UnitAuthoring : MonoBehaviour
{
    public FactionType faction;
    class UnitAuthoringBaker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit(){});
        }
    }
}

public struct Unit : IComponentData
{
}
