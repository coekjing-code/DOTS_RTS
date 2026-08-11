using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
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

        Debug.Log("Can not find AnimationTypeSO for AnimationType : " + animationType);

        return null;
    }
}