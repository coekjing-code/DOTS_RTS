using System;
using Unity.Collections;
using Unity.Entities;
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
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Unit>().Build(entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float currentSelectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float minimumMultipleSelectionAreaSize = 50f;
            bool isMultipleSelect;
            // 多选判定
            if (currentSelectionAreaSize > minimumMultipleSelectionAreaSize)
            {
                isMultipleSelect = true;
            }
            else
            {
                isMultipleSelect = false;
                Debug.Log("MultipleSelect : " + isMultipleSelect);
            }
            
            
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
                    }
                }
            }
            else
            {
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                
                // units所属层级
                int unitsLayer = 6;

                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastInput raycastInput = new RaycastInput()
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << unitsLayer,
                        GroupIndex = 0,
                    }    
                };

                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                    }
                }
            }


            onSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = mousePosition;
                entityManager.SetComponentData(entityArray[i], unitMover);
                unitMoverArray[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;

        Vector2 leftLower = new (Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x), Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y));
        Vector2 rightUpper = new (Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x), Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y)); 

        return new Rect(leftLower.x, leftLower.y, rightUpper.x - leftLower.x, rightUpper.y - leftLower.y);
    }
}