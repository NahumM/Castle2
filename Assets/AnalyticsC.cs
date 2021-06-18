using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsC : MonoBehaviour
{
    public void AnalyticsLevelStart(int levelNumber)
    {
        Debug.Log("Level " + levelNumber + " started! (Analytics)");
    }

    public void AnalyticsLevelRestart(int levelNumber)
    {
        Debug.Log("Level " + levelNumber + " restarted! (Analytics)");
    }
    public void AnalyticsLevelVictory(int levelNumber)
    {
        Debug.Log("Level " + levelNumber + " is passed! (Analytics)");
    }
    public void AnalyticsLevelLoss(int levelNumber)
    {
        Debug.Log("Level " + levelNumber + " failed! (Analytics)");
    }
}
