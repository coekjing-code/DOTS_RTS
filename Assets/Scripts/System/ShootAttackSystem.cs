using Unity.Burst;
using Unity.Entities;

partial struct ShootAttackSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<ShootAttack> shootAttack, RefRO<Target> target) in SystemAPI.Query<RefRW<ShootAttack>, RefRO<Target>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;

            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (shootAttack.ValueRW.timer > 0f) continue;

            shootAttack.ValueRW.timer = shootAttack.ValueRO.timerMax;

            int shootDamage = 1;

            RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
            targetHealth.ValueRW.health -= shootDamage;
        }
    }
}
