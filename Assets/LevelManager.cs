using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    List<CastleBehaviour> allCastles = new List<CastleBehaviour>();
    int castlesInLevel;
    int currentLevelID;
    GameObject currentLevel;
    [SerializeField] List<GameObject> levels = new List<GameObject>();

    private void Start()
    {
        currentLevelID = PlayerPrefs.GetInt("Level");
        StartCoroutine("RestartDelay", false);
    }
    public void CastleCapture()
    {
        CastleBehaviour.Belongs castleBelongs = CastleBehaviour.Belongs.Empty;
        for (int i = 0; i < allCastles.Count; i++)
        {
            if (i == 0)
            {
                castleBelongs = allCastles[i].castleBelongs;
            } else
            {
                if (castleBelongs != allCastles[i].castleBelongs)
                {
                    return;
                }
            }
        }
        if (castleBelongs == CastleBehaviour.Belongs.Enemy)
        {
            LevelLost();
        }
        if (castleBelongs == CastleBehaviour.Belongs.Player)
        {
            LevelWin();
        }
    }


    IEnumerator RestartDelay(bool delay)
    {
        if (delay) yield return new WaitForSeconds(1f);
        Destroy(currentLevel);
        currentLevel = Instantiate(levels[currentLevelID]);
        AccureAllCastles();
    }

    IEnumerator LoadDelay()
    {
        yield return new WaitForSeconds(1f);
        Destroy(currentLevel);
        if (levels.Count - 1 > currentLevelID) currentLevelID++;
        currentLevel = Instantiate(levels[currentLevelID]);
        AccureAllCastles();
    }

    void AccureAllCastles()
    {
        allCastles.AddRange(GameObject.FindObjectsOfType<CastleBehaviour>());
        foreach (CastleBehaviour castle in allCastles)
        {
            castle.levelManager = this;
        }
        castlesInLevel = allCastles.Count;
    }
    void LevelWin()
    {
        Debug.Log("You won the level!");
        StartCoroutine("LoadDelay");
    }

    void LevelLost()
    {
        Debug.Log("You lost the level!");
        StartCoroutine("RestartDelay", true);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("Level", currentLevelID);
        PlayerPrefs.Save();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PlayerPrefs.SetInt("Level", currentLevelID);
            PlayerPrefs.Save();
        }
    }
}
