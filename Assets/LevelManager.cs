using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    List<CastleBehaviour> allCastles = new List<CastleBehaviour>();
    int castlesInLevel;
    [SerializeField] int currentLevelID;
    GameObject currentLevel;
    [SerializeField] List<GameObject> levels = new List<GameObject>();

    private void Start()
    {
        currentLevelID = PlayerPrefs.GetInt("Level");
        if (currentLevelID < 0) currentLevelID = 0;
        StartCoroutine("RestartDelay", false);
    }
    public void CastleCapture()
    {
        StopAllCoroutines();
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
        if (delay)
        {
            yield return new WaitForSeconds(1f);
            GameObject[] allyArmies = GameObject.FindGameObjectsWithTag("PlayerArmy");
            if (allyArmies.Length > 0)
            {
                Debug.Log("Armies not null");
                StartCoroutine("RestartDelay", true);
                yield break;
            }
        }

        Destroy(currentLevel);
        currentLevel = Instantiate(levels[currentLevelID]);
        AccureAllCastles();
    }

    IEnumerator LoadDelay()
    {
        yield return new WaitForSeconds(1f);
        GameObject[] enemyArmies = GameObject.FindGameObjectsWithTag("EnemyArmy");
        if (enemyArmies.Length > 0)
        {
            StartCoroutine("LoadDelay");
            yield return null;
        }
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
        StartCoroutine("LoadDelay");
    }

    void LevelLost()
    {
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
