using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu()]
public class UnitTypeSO : ScriptableObject
{
    public enum UnitType
    {
        None,
        Soldier,
        Scout,
        Zombie
    }

    public UnitType unitType;
    public float progressMax;
    public Sprite sprite; 

    public Entity GetPrefabEntity(EntitiesReferences entitiesReferences)
    {
        switch (unitType)
        {
            default:
            case UnitType.None:
            case UnitType.Soldier:
                return entitiesReferences.soldierPrefabEntity;
            case UnitType.Scout:
                return entitiesReferences.scoutPrefabEntity;
            case UnitType.Zombie:
                return entitiesReferences.zombiePrefabEntity;
        }
    }
}
