using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // 如果EntitesReferences不存在，则不执行OnUpdate方法
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        foreach ((
            RefRW<LocalTransform> localTransform,
            RefRW<ShootAttack> shootAttack,
            RefRO<Target> target,
            RefRW<UnitMover> unitMover,
            Entity entity)
            in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<ShootAttack>,
                RefRO<Target>,
        // 移动等级高于攻击等级
        RefRW<UnitMover>>().WithDisabled<MoveOverride>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > shootAttack.ValueRO.attackDistance)
            {
                // 距离过远则不射击
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
                continue;
            }
            else
            {
                // 到达射击距离后停下
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            }

            // 射击时转向目标
            float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);
            quaternion rotation = quaternion.LookRotation(aimDirection, math.up());
            localTransform.ValueRW.Rotation =
            math.slerp(localTransform.ValueRO.Rotation, rotation, SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotateSpeed);

        }


        foreach ((
            RefRW<LocalTransform> localTransform,
            RefRW<ShootAttack> shootAttack,
            RefRO<Target> target,
            Entity entity)
            in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<ShootAttack>,
                RefRO<Target>>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > shootAttack.ValueRO.attackDistance)
            {
                continue;
            }

            if (SystemAPI.HasComponent<MoveOverride>(entity) && SystemAPI.IsComponentEnabled<MoveOverride>(entity))
            {
                continue;
            }

            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (shootAttack.ValueRW.timer > 0f)
            {
                continue;
            }

            shootAttack.ValueRW.timer = shootAttack.ValueRO.timerMax;

            // Unit射击敌人或者建筑时，若敌人targetOverride为Null,则将Unit设为其targetOverride
            if (SystemAPI.HasComponent<TargetOverride>(target.ValueRO.targetEntity))
            {
                RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.targetEntity);
                if (enemyTargetOverride.ValueRO.targetEntity == Entity.Null)
                {
                    enemyTargetOverride.ValueRW.targetEntity = entity;
                }
            }

            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);
            // 将子弹生成位置由局部坐标转为世界坐标
            float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

            // 将子弹的伤害设置为ShootAttack的伤害
            RefRW<Bullet> bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bullet.ValueRW.damageAmount = shootAttack.ValueRW.damageAmount;

            // 将子弹的目标设为ShootAttack的目标
            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;

            shootAttack.ValueRW.onShoot.isTriggered = true;
            shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;
        }
    }
}
