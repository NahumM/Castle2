using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialHandler : MonoBehaviour
{
    bool isTrainingPassed;
    GameObject hand;
    TextMeshProUGUI text;
    Animator anim;


    private void Start()
    {
        if (!isTrainingPassed)
            StartTraining();
    }

    void StartTraining()
    {
        hand.SetActive(true);
        text.gameObject.SetActive(true);
    }
}
