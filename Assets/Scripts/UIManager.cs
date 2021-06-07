using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] LevelManager levelM;
    [SerializeField] GameObject restartButtons;
    [SerializeField] GameObject winButtons;

    public void ShowRestartButtonUI(bool onOFF)
    {
        restartButtons.SetActive(onOFF);
    }

    public void ShowWinButtonUI(bool onOFF)
    {
            winButtons.SetActive(onOFF);
    }

    public void PressRestartButton()
    {
        levelM.RestartLevel();
    }

    public void PressWinButton()
    {
        levelM.LoadLevel();
    }

    public void PressRewardButton()
    {
        Debug.Log("Reward button pressed!");
    }
}
