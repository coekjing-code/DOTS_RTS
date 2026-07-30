using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    private Vector2 selectionStartMousePosition;
    public EventHandler onSelectionAreaStart;
    public EventHandler onSelectionAreaEnd;
    
    public static UnitSelectionManager Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;
            onSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }
        if (Input.GetMouseButtonUp(0))
        {

            Vector2 selectionEndMousePosition = Input.mousePosition;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            // 先清除原先选中
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                selectedArray[i] = selected;

                entityManager.SetComponentData(entityArray[i], selected);
            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float currentSelectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float minimumMultipleSelectionAreaSize = 40f;
            // 多选判定
            bool isMultipleSelect = currentSelectionAreaSize > minimumMultipleSelectionAreaSize;

            // 再重新选中
            if (isMultipleSelect)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);
                NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransforms = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < localTransforms.Length; i++)
                {
                    LocalTransform localTransform = localTransforms[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(localTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entities[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entities[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entities[i], selected);
                    }
                }
            }
            else
            {
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                

                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastInput raycastInput = new RaycastInput()
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNIT_LAYER,
                        GroupIndex = 0,
                    }    
                };

                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        // 击中单位
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);
                    }
                }
            }


            onSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastInput raycastInput = new RaycastInput()
            {
                Start = cameraRay.GetPoint(0f),
                End = cameraRay.GetPoint(9999f),
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.UNIT_LAYER,
                    GroupIndex = 0,
                }
            };

            // 区分此次右键是选择敌人还是移动位置
            bool isAttackingSingleTarget = false;
            if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
            {
                if (entityManager.HasComponent<Unit>(raycastHit.Entity))
                {
                    // 右键点中僵尸时，将其设为overrideTarget
                    Unit unit = entityManager.GetComponentData<Unit>(raycastHit.Entity);
                    if (unit.faction == Faction.Zombie)
                    {
                        isAttackingSingleTarget = true;

                        // 查询所有的被选中的Unit(带有TargetOverride组件)
                        entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<TargetOverride>().Build(entityManager);

                        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                        NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                        // 将这些Unit的TargetOverride设为右键选中的敌人
                        for (int i = 0; i < targetOverrideArray.Length; i++)
                        {
                            TargetOverride targetOverride = targetOverrideArray[i];
                            targetOverride.targetEntity = raycastHit.Entity;
                            targetOverrideArray[i] = targetOverride;
                            // 并禁用MoveOverride组件
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                        }
                        entityQuery.CopyFromComponentDataArray(targetOverrideArray);
                    }
                }
            }

            if (!isAttackingSingleTarget)
            {
                // 查询所有带有MoveOverride和TargetOverride组件的Unit
                entityQuery = new EntityQueryBuilder(Allocator.Temp).
                WithAll<Selected>().WithPresent<MoveOverride, TargetOverride>().Build(entityManager);

                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<MoveOverride> moverOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
                NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                NativeArray<float3> movePositionArray = GenerateMovePositionArray(mousePosition, entityArray.Length);
                // 设置MoveOverride的TargetPosition,启用MoveOverride
                // 更新查询到的Entity的组件
                // 将TargetOverride的targetEntity设为Null
                for (int i = 0; i < moverOverrideArray.Length; i++)
                {
                    MoveOverride moveOverride = moverOverrideArray[i];
                    moveOverride.targetPosition = movePositionArray[i];
                    entityManager.SetComponentData(entityArray[i], moveOverride);   
                    moverOverrideArray[i] = moveOverride;
                    entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);

                    TargetOverride targetOverride = targetOverrideArray[i];
                    targetOverride.targetEntity = Entity.Null;
                    targetOverrideArray[i] = targetOverride;
                }
                entityQuery.CopyFromComponentDataArray(moverOverrideArray);
                entityQuery.CopyFromComponentDataArray(targetOverrideArray);
            }
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;

        Vector2 leftLower = new (Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x), Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y));
        Vector2 rightUpper = new (Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x), Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y)); 

        return new Rect(leftLower.x, leftLower.y, rightUpper.x - leftLower.x, rightUpper.y - leftLower.y);
    }

    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);

        if (positionCount == 0) return positionArray;

        positionArray[0] = targetPosition;

        if (positionCount == 1) return positionArray;

        float ringRadius = 1.5f;
        int positionIndex = 1;
        int ringCount = 0;

        while (positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ringCount * 2;

            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * math.PI2 / ringPositionCount;
                float3 ringVector3 = math.rotate(quaternion.RotateY(angle), new float3(ringRadius * (ringCount + 1), 0f, 0f));
                float3 ringPosition = targetPosition + ringVector3;

                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount)
                {
                    break;
                }
            }
            ringCount++;
        }
        return positionArray;
    }
}