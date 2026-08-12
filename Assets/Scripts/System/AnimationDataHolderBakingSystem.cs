using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[UpdateInGroup(typeof(PostBakingSystemGroup))]
partial struct AnimationDataHolderBakingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        AnimationDataListSO animationDataListSO = null;

        foreach (RefRO<AnimationDataHolderObjectData> animationDataHolderObjectData in SystemAPI.Query<RefRO<AnimationDataHolderObjectData>>())
        {
            animationDataListSO = animationDataHolderObjectData.ValueRO.animationDataListSO.Value;
        }
        Dictionary<AnimationDataSO.AnimationType, int[]> blobAssetDataDictionary = new Dictionary<AnimationDataSO.AnimationType, int[]>();

        foreach (AnimationDataSO.AnimationType animationType in System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)))
        {
            AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
            blobAssetDataDictionary[animationType] = new int[animationDataSO.meshArray.Length];
        }

        foreach ((
            RefRO<AnimationDataHolderSubEntity> animationDataHolderSubEntity,
            RefRO<MaterialMeshInfo> materialMeshInfo)
            in SystemAPI.Query<
                RefRO<AnimationDataHolderSubEntity>,
                RefRO<MaterialMeshInfo>>())
        {
            blobAssetDataDictionary[animationDataHolderSubEntity.ValueRO.animationType]
                [animationDataHolderSubEntity.ValueRO.meshIndex] = materialMeshInfo.ValueRO.Mesh;
        }

        foreach (RefRW<AnimationDataHolder> animationDataHolder
            in SystemAPI.Query<RefRW<AnimationDataHolder>>())
        {
            // BlobBuilder 是临时构建器，只用于拼装 Blob 数据，本身不是 Blob 数据。
            // Allocator.Temp：Baker 是编辑器主线程 ，Temp 合法；构建完必须Dispose()。
            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);

            // BlobBuilder 销毁之后，你拿到的 BlobAssetReference 依然有效，真正 Blob 内存是在CreateBlobAssetReference指定的Allocator.Persistent上。
            ref BlobArray<AnimationData> animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();

            // Allocate 给根 BlobArray 分配元素数量
            // ref animationDataBlobArray：传入根的 BlobArray 引用，告诉 Builder 要给这个数组分配 N 个元素。
            // 返回值 BlobBuilderArray<AnimationData>：写入句柄，构建阶段唯一合法写入手段。
            BlobBuilderArray<AnimationData> animationDataBlobBuilderArray =
                blobBuilder.Allocate<AnimationData>(ref animationDataBlobArray, System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)).Length);

            int index = 0;
            foreach (AnimationDataSO.AnimationType animationType in System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)))
            {
                AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);

                // 嵌套 BlobArray 分配（数组里面又套数组）
                BlobBuilderArray<int> blobBuilderArray =
                    blobBuilder.Allocate<int>(ref animationDataBlobBuilderArray[index].
                        intMeshIdBlobArray, animationDataSO.meshArray.Length);

                animationDataBlobBuilderArray[index].frameTimerMax = animationDataSO.frameTimerMax;
                animationDataBlobBuilderArray[index].frameMax = animationDataSO.meshArray.Length;

                for (int i = 0; i < animationDataSO.meshArray.Length; i++)
                {
                    blobBuilderArray[i] = blobAssetDataDictionary[animationType][i];
                }

                index++;
            }
            // CreateBlobAssetReference：真正生成 Blob 内存
            animationDataHolder.ValueRW.animationDataBlobArrayBlobAssetReference =
                blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
            blobBuilder.Dispose();
        }
    }
    
    public void OnDestroy(ref SystemState state)
    {
        foreach (RefRW<AnimationDataHolder> animationDataHolder in SystemAPI.Query<RefRW<AnimationDataHolder>>())
        {
            animationDataHolder.ValueRO.animationDataBlobArrayBlobAssetReference.Dispose();
        }
    }
}
