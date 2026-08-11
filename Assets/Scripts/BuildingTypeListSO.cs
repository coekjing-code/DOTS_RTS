using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BuidlingTypeListSO : ScriptableObject
{
    public List<BuildingTypeSO> buildingTypeSOList;

    public BuildingTypeSO GetBuildingTypeSO(BuildingTypeSO.BuildingType buildingType)
    {
        foreach (BuildingTypeSO buildingTypeSO in buildingTypeSOList)
        {
            if (buildingTypeSO.buildingType == buildingType)
            {
                return buildingTypeSO;
            }
        }

        Debug.Log("Could not find BuildingTypeSO for BuildingType : " + buildingType);
        return null;
    }
}