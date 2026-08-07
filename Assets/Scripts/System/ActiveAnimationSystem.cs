using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

partial struct ActiveAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationDataHolder>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AnimationDataHolder animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

        ActiveAnimationJob activeAnimationJob = new ActiveAnimationJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            animationDataBlobArrayBlobAssetReference = animationDataHolder.animationDataBlobArrayBlobAssetReference,
        };

        activeAnimationJob.ScheduleParallel();

        // foreach ((RefRW<ActiveAnimation> activeAnimation, RefRW<MaterialMeshInfo> materialMeshInfo) in
        //     SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>())
        // {
        //     ref AnimationData animationData = 
        //         ref animationDataHolder.animationDataBlobArrayBlobAssetReference.Value
        //             [(int)activeAnimation.ValueRO.activeAnimationType];

        //     activeAnimation.ValueRW.frameTimer += SystemAPI.Time.DeltaTime;
        //     if (activeAnimation.ValueRO.frameTimer >= animationData.frameTimerMax)
        //     {
        //         activeAnimation.ValueRW.frameTimer -= animationData.frameTimerMax;
        //         activeAnimation.ValueRW.frame = 
        //             (activeAnimation.ValueRO.frame + 1) % animationData.frameMax;

        //         materialMeshInfo.ValueRW.MeshID = animationData.batchMeshIdBlobArray[activeAnimation.ValueRO.frame];

        //         // 如果播放完射击动画，就跳转到None，再从None跳转到其他
        //         if (activeAnimation.ValueRO.frame == 0 && activeAnimation.ValueRO.activeAnimationType == AnimationDataSO.AnimationType.SoldierShoot)
        //         {
        //             activeAnimation.ValueRW.nextAnimationType = AnimationDataSO.AnimationType.None;
        //         }

        //         // 如果播放完近战动画，就跳转到None，再从None跳转到其他
        //         if (activeAnimation.ValueRO.frame == 0 && activeAnimation.ValueRO.activeAnimationType == AnimationDataSO.AnimationType.ZombieAttack)
        //         {
        //             activeAnimation.ValueRW.nextAnimationType = AnimationDataSO.AnimationType.None;
        //         }
        //     }
        // }
    }
}

public partial struct ActiveAnimationJob : IJobEntity
{
    public float deltaTime;
    public BlobAssetReference<BlobArray<AnimationData>> animationDataBlobArrayBlobAssetReference;
    public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
    {
        ref AnimationData animationData = 
            ref animationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.activeAnimationType];

        activeAnimation.frameTimer += deltaTime;
        if (activeAnimation.frameTimer >= animationData.frameTimerMax)
        {
            activeAnimation.frameTimer -= animationData.frameTimerMax;
            activeAnimation.frame = 
                (activeAnimation.frame + 1) % animationData.frameMax;
            materialMeshInfo.Mesh = animationData.intMeshIdBlobArray[activeAnimation.frame];

            // 当播放完不可打断的动画后，跳转到None
            if (activeAnimation.frame == 0 && AnimationDataSO.IsAnimationUninterruptible(activeAnimation.activeAnimationType))
            {
                activeAnimation.nextAnimationType = AnimationDataSO.AnimationType.None;
            }
        }
    }
}