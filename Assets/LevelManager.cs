using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    List<CastleBehaviour> allCastles = new List<CastleBehaviour>();
    int castlesInLevel;
    private void Start()
    {
        AccureAllCastles();
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
                    Debug.Log("Castle not!");
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


    IEnumerator RestartDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
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
        StartCoroutine("RestartDelay");
    }

    void LevelLost()
    {
        Debug.Log("You lost the level!");
        StartCoroutine("RestartDelay");
    }
}
