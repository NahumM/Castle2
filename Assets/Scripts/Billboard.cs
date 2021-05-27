using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    TextMeshProUGUI textCount;
    Slider slider;
    
    void Awake()
    {
        if (transform.childCount != 1)
            slider = transform.GetChild(0).GetComponent<Slider>();
        if (transform.childCount != 1)
            textCount = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        else textCount = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }

    public void SetValue(int value)
    {
        textCount.text = value.ToString();
        if (slider != null)
            slider.value = value;
    }    

    public void SetMaxValue(int value)
    {
        slider.maxValue = value;
    }

}
