using UnityEngine;

[CreateAssetMenu(fileName = "AnimationDataSO")]
public class AnimationDataSO : ScriptableObject
{
    public enum AnimationType
    {
        None,
        SoldierIdle,
        SoldierWalk,
        ZombieIdle,
        ZombieWalk,
        SoldierShoot,
        SoldierAim,
    }

    public AnimationType animationType;
    public Mesh[] meshArray;
    public float frameTimerMax;
}