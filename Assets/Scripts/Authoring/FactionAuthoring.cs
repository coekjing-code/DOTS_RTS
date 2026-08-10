using Unity.Entities;
using UnityEngine;

class FactionAuthoring : MonoBehaviour
{
    public FactionType factionType;

    class FactionAuthoringBaker : Baker<FactionAuthoring>
    {
        public override void Bake(FactionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Faction()
            {
                factionType = authoring.factionType,
            });            
        }
    }
}

public struct Faction : IComponentData
{
    public FactionType factionType;
}


