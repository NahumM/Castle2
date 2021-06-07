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
    [SerializeField] UIManager ui;

    private void Start()
    {
        currentLevelID = PlayerPrefs.GetInt("Level");
        if (currentLevelID < 0) currentLevelID = 0;
        LoadLevel();
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
            StartCoroutine("RestartDelay");
        }
        if (castleBelongs == CastleBehaviour.Belongs.Player)
        {
            StartCoroutine("LoadDelay");
        }
    }


    IEnumerator RestartDelay()
    {
            yield return new WaitForSeconds(1f);
            GameObject[] allyArmies = GameObject.FindGameObjectsWithTag("PlayerArmy");
            if (allyArmies.Length > 0)
            {
                Debug.Log("Armies not null");
                StartCoroutine("RestartDelay", true);
                yield break;
            }
        ui.ShowRestartButtonUI(true);
        GameObject.FindObjectOfType<PlayerController>().isGameEnded = true;
    }

    public void RestartLevel()
    {
        Destroy(currentLevel);
        currentLevel = Instantiate(levels[currentLevelID]);
        AccureAllCastles();
        ui.ShowRestartButtonUI(false);
    }

    public void LoadLevel()
    {
        Destroy(currentLevel);
        if (levels.Count - 1 > currentLevelID) currentLevelID++;
        else currentLevelID = 0;
        currentLevel = Instantiate(levels[currentLevelID]);
        AccureAllCastles();
        ui.ShowWinButtonUI(false);
    }

    IEnumerator LoadDelay()
    {
        yield return new WaitForSeconds(1f);
        GameObject[] enemyArmies = GameObject.FindGameObjectsWithTag("EnemyArmy");
        if (enemyArmies.Length > 0)
        {
            StartCoroutine("LoadDelay");
            yield break;
        }
        ui.ShowWinButtonUI(true);
        GameObject.FindObjectOfType<PlayerController>().isGameEnded = true;
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
