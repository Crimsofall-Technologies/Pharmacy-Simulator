using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int currentXp, maxXp, lastXp, Level, Cash, Gems;

    //Researches:
    public float[] researchRemainTimes;
    public bool[] ResearchesComplete;

    //Aisles
    public int remainGroceries, remainIcecream, remainPharmacy, remainVaccine, remainDrinks, remainBackrooms;

    //Buildings:
    public bool[] CompletedBuildings;
    public int[] BuildingUpdateIndexes;
    public float[] BuildingRemainTime;

    //Perks
    public float remainDoubleTime, remainGuard, remainHelper, remainDoubleMoney;

    //Achivements
    public int[] AchivementsProgress;
    public bool[] AchivementsCollected;

    public bool TutorComplete;
}
