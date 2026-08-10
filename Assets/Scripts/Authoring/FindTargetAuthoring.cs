using Unity.Entities;
using UnityEngine;

class FindTargetAuthoring : MonoBehaviour
{
    public float range;
    public FactionType faction;
    public float timerMax;

    class FindTargetAuthoringBaker : Baker<FindTargetAuthoring>
    {
        public override void Bake(FindTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FindTarget()
            {
                range = authoring.range,
                faction = authoring.faction,
                timerMax = authoring.timerMax,
            });
        }
    }
}

public struct FindTarget : IComponentData
{
    public float range;
    public FactionType faction;
    public float timer;
    public float timerMax;
}

