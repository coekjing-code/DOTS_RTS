using Unity.Entities;
using UnityEngine;

class MeleeAttackAuthoring : MonoBehaviour
{
    public float timerMax;
    public int damageAmount;
    public float collideSize;
    class MeleeAttackAuthoringBaker : Baker<MeleeAttackAuthoring>
    {
        public override void Bake(MeleeAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttack()
            {
                timerMax = authoring.timerMax,
                damageAmount = authoring.damageAmount,
                collideSize = authoring.collideSize,
            });
        }
    }
}

public struct MeleeAttack : IComponentData
{
    public float timer;
    public float timerMax;
    public int damageAmount;
    public float collideSize;
}

