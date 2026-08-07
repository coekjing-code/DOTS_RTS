using Unity.Entities;
using UnityEngine;

class UnitAnimationsAuthoring : MonoBehaviour
{
    public AnimationDataSO.AnimationType idleAnimationType;
    public AnimationDataSO.AnimationType walkAnimationType;
    public AnimationDataSO.AnimationType shootAnimationType;
    public AnimationDataSO.AnimationType aimAnimationType;
    public AnimationDataSO.AnimationType meleeAnimationType;


    class UnitAnimationsAuthoringBaker : Baker<UnitAnimationsAuthoring>
    {
        public override void Bake(UnitAnimationsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitAnimations()
            {
                idleAnimationType = authoring.idleAnimationType,
                walkAnimationType = authoring.walkAnimationType,
                shootAnimationType = authoring.shootAnimationType,
                aimAnimationType = authoring.aimAnimationType,
                meleeAnimationType = authoring.meleeAnimationType,
            });
        }
    }
}

public struct UnitAnimations : IComponentData
{
    public AnimationDataSO.AnimationType idleAnimationType;
    public AnimationDataSO.AnimationType walkAnimationType;
    public AnimationDataSO.AnimationType shootAnimationType;
    public AnimationDataSO.AnimationType aimAnimationType;
    public AnimationDataSO.AnimationType meleeAnimationType;
}
