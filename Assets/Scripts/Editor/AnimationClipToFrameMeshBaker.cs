using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationClipToFrameMeshBaker : EditorWindow
{
    [Header("源对象（必须带有SkinnedMeshRenderer）")]
    private GameObject sourceModel;
    [Header("待烘焙动画Clip")]
    private AnimationClip targetClip;
    [Header("采样帧率（每秒多少帧网格）")]
    private int sampleFps = 30;
    [Header("输出保存路径（相对Assets）")]
    private string outputPath = "Assets/BakedFrameMeshes";

    private bool isBaking = false;

    [MenuItem("Tools/动画逐帧网格烘焙工具")]
    static void OpenWindow()
    {
        GetWindow<AnimationClipToFrameMeshBaker>("动画网格烘焙器");
    }

    private void OnGUI()
    {
        sourceModel = (GameObject)EditorGUILayout.ObjectField("蒙皮模型", sourceModel, typeof(GameObject), true);
        targetClip = (AnimationClip)EditorGUILayout.ObjectField("AnimationClip", targetClip, typeof(AnimationClip), false);
        sampleFps = EditorGUILayout.IntSlider("采样帧率", sampleFps, 1, 60);
        outputPath = EditorGUILayout.TextField("输出目录", outputPath);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(isBaking || sourceModel == null || targetClip == null))
        {
            if (GUILayout.Button("开始烘焙逐帧网格"))
            {
                BakeAnimationToFrameMeshes();
            }
        }

        if (isBaking)
        {
            EditorGUILayout.HelpBox("正在烘焙，请等待...不要操作Unity", MessageType.Info);
        }
    }

    void BakeAnimationToFrameMeshes()
    {
        isBaking = true;
        // 创建输出目录
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // 复制实例，防止修改原始场景对象
        GameObject bakeObj = Instantiate(sourceModel);
        SkinnedMeshRenderer skinnedRenderer = bakeObj.GetComponent<SkinnedMeshRenderer>();
        if (skinnedRenderer == null)
        {
            EditorUtility.DisplayDialog("错误", "选中模型不存在 SkinnedMeshRenderer！", "OK");
            DestroyImmediate(bakeObj);
            isBaking = false;
            return;
        }

        float clipDuration = targetClip.length;
        float frameInterval = 1f / sampleFps;
        int totalFrameCount = Mathf.CeilToInt(clipDuration * sampleFps);

        // 逐帧采样
        for (int frameIndex = 0; frameIndex < totalFrameCount; frameIndex++)
        {
            float time = frameIndex * frameInterval;
            if (time > clipDuration) time = clipDuration;

            // 采样动画到对象骨骼
            targetClip.SampleAnimation(bakeObj, time);

            // 烘焙当前姿态网格
            Mesh bakedMesh = new Mesh();
            skinnedRenderer.BakeMesh(bakedMesh);

            // 复制数据（避免网格数据被复用覆盖）
            Mesh saveMesh = Instantiate(bakedMesh);
            saveMesh.name = $"Frame_{frameIndex:D4}";

            // 保存到Asset
            string assetFullPath = $"{outputPath}/{saveMesh.name}.asset";
            AssetDatabase.CreateAsset(saveMesh, assetFullPath);

            EditorUtility.DisplayProgressBar("烘焙动画网格", $"帧 {frameIndex}/{totalFrameCount}", (float)frameIndex / totalFrameCount);
        }

        // 清理
        DestroyImmediate(bakeObj);
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        isBaking = false;
        EditorUtility.DisplayDialog("完成", $"烘焙结束！共生成 {totalFrameCount} 个网格\n保存路径：{outputPath}", "OK");
    }
}