using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeList<RaycastHit> raycastHits = new NativeList<RaycastHit>(Allocator.Temp);

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        foreach ((
        RefRO<LocalTransform> localTransform,
        RefRW<MeleeAttack> meleeAttack,
        RefRO<Target> target,
        RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
            RefRO<LocalTransform>,
            RefRW<MeleeAttack>,
            RefRO<Target>,
            RefRW<UnitMover>>().WithDisabled<MoveOverride>())
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            float minMeleeAttackDistanceSq = 2f;

            bool isCloseEnoughToAttack = math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position) < minMeleeAttackDistanceSq;
            bool isTouchingTarget = false;

            if (!isCloseEnoughToAttack)
            {
                float3 castDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
                castDirection = math.normalize(castDirection);
                float extractDistanceToTestRayCast = 0.4f;
                RaycastInput raycastInput = new RaycastInput()
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position + castDirection * (meleeAttack.ValueRO.collideSize + extractDistanceToTestRayCast),
                    Filter = CollisionFilter.Default,
                };

                raycastHits.Clear();
                if (collisionWorld.CastRay(raycastInput, ref raycastHits))
                {
                    foreach (RaycastHit raycastHit in raycastHits)
                    {
                        if (raycastHit.Entity == target.ValueRO.targetEntity)
                        {
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }
            
            if (!isCloseEnoughToAttack && !isTouchingTarget)
            {
                // 若没有达到攻击距离且没有触碰到target，则不进攻
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
            }
            else
            {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (meleeAttack.ValueRO.timer > 0f) continue;

                meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;
            }
        }
    }
}
