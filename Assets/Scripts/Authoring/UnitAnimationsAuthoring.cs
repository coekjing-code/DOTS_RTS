using Unity.Entities;
using UnityEngine;

class UnitAnimationsAuthoring : MonoBehaviour
{
    public AnimationDataSO.AnimationType idleAnimationType;
    public AnimationDataSO.AnimationType walkAnimationType;

    class UnitAnimationsAuthoringBaker : Baker<UnitAnimationsAuthoring>
    {
        public override void Bake(UnitAnimationsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitAnimations()
            {
                idleAnimationType = authoring.idleAnimationType,
                walkAnimationType = AnimationDataSO.AnimationType.SoldierWalk,
            });
        }
    }
}

public struct UnitAnimations : IComponentData
{
    public AnimationDataSO.AnimationType idleAnimationType;
    public AnimationDataSO.AnimationType walkAnimationType;
}
