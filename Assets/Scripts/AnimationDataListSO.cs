using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationDataListSO")]
public class AnimationDataListSO : ScriptableObject
{
    public List<AnimationDataSO> animationDataSOList;

    public AnimationDataSO GetAnimationDataSO(AnimationDataSO.AnimationType animationType)
    {
        foreach (var animationDataSO in animationDataSOList)
        {
            if (animationDataSO.animationType == animationType)
            {
                return animationDataSO;
            }
        }

        Debug.Log("Can not find the target animation type : " + animationType);

        return null;        
    }
}