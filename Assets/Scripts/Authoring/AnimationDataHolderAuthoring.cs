using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

class AnimationDataHolderAuthoring : MonoBehaviour
{
    public AnimationDataListSO animationDataListSO;
    class AnimationDataHolderAuthoringBaker : Baker<AnimationDataHolderAuthoring>
    {
        public override void Bake(AnimationDataHolderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AnimationDataHolder animationDataHolder = new AnimationDataHolder();

            EntitiesGraphicsSystem entitiesGraphicsSystem =
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            // BlobBuilder 是临时构建器，只用于拼装 Blob 数据，本身不是 Blob 数据。
            // Allocator.Temp：Baker 是编辑器主线程，Temp 合法；构建完必须Dispose()。
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
                AnimationDataSO animationDataSO = authoring.animationDataListSO.GetAnimationDataSO(animationType);

                // 嵌套 BlobArray 分配（数组里面又套数组）
                BlobBuilderArray<BatchMeshID> blobBuilderArray =
                    blobBuilder.Allocate<BatchMeshID>(ref animationDataBlobBuilderArray[index].
                        batchMeshIdBlobArray, animationDataSO.meshArray.Length);

                animationDataBlobBuilderArray[index].frameTimerMax = animationDataSO.frameTimerMax;
                animationDataBlobBuilderArray[index].frameMax = animationDataSO.meshArray.Length;

                for (int i = 0; i < animationDataSO.meshArray.Length; i++)
                {
                    Mesh mesh = animationDataSO.meshArray[i];
                    blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
                }

                index++;
            }
            // CreateBlobAssetReference：真正生成 Blob 内存
            animationDataHolder.animationDataBlobArrayBlobAssetReference =
                blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
            blobBuilder.Dispose();
            AddBlobAsset(ref animationDataHolder.animationDataBlobArrayBlobAssetReference, out Unity.Entities.Hash128 objectHash);
            
            AddComponent(entity, animationDataHolder);
        }
    }
}

public struct AnimationDataHolder : IComponentData
{
    public BlobAssetReference<BlobArray<AnimationData>> animationDataBlobArrayBlobAssetReference;
    public BlobAssetReference<AnimationData> soldierWalk;
}

public struct AnimationData
{
    public float frameTimerMax;
    public int frameMax;
    public BlobArray<BatchMeshID> batchMeshIdBlobArray;
}