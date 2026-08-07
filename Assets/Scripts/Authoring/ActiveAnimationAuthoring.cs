using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

class ActiveAnimationAuthoring : MonoBehaviour
{
    public AnimationDataSO.AnimationType nextAnimationType;

    class ActiveAnimationAuthoringBaker : Baker<ActiveAnimationAuthoring>
    {
        public override void Bake(ActiveAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic); 

            AddComponent(entity, new ActiveAnimation()
            {
                nextAnimationType = authoring.nextAnimationType,
            });
        }
    }
}

public struct ActiveAnimation : IComponentData
{
    public int frame;
    public float frameTimer;
    public AnimationDataSO.AnimationType activeAnimationType;
    public AnimationDataSO.AnimationType nextAnimationType;
}