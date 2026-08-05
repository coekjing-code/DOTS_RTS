using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    public const float REACH_TARGET_POSITION_SQ = 0.5f;
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };

        unitMoverJob.ScheduleParallel();

        // foreach ((RefRW<LocalTransform> localTransform, RefRO<UnitMover> unitMover, RefRW<PhysicsVelocity> physicsVelocity) 
        // in SystemAPI.Query<RefRW<LocalTransform>, RefRO<UnitMover>, RefRW<PhysicsVelocity>>())
        // {
        //     float3 moveDirection = unitMover.ValueRO.targetPosition - localTransform.ValueRW.Position;
        //     moveDirection = math.normalize(moveDirection);

        //     localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRW.Rotation, quaternion.LookRotation(moveDirection, math.up()), 
        //     unitMover.ValueRO.rotateSpeed * SystemAPI.Time.DeltaTime);
            
        //     physicsVelocity.ValueRW.Angular = float3.zero;
        //     physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
        // }
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    public void Execute(ref LocalTransform localTransform, ref UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        float reachTargetDistanceSq = UnitMoverSystem.REACH_TARGET_POSITION_SQ;
        if (math.lengthsq(moveDirection) <= reachTargetDistanceSq)
        {
            physicsVelocity.Angular = float3.zero;
            physicsVelocity.Linear = float3.zero;
            unitMover.isMoving = false;
            return;
        }

        unitMover.isMoving = true;
        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation = math.slerp(localTransform.Rotation, quaternion.LookRotation(moveDirection, math.up()), 
        unitMover.rotateSpeed * deltaTime);

        physicsVelocity.Angular = float3.zero;
        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
    }
}
