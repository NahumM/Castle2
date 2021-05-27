using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkTwin : MonoBehaviour
{
    Renderer glow;
    bool goBackward;
    Color oldColor;
    // 30 - 130
    void Start()
    {
        glow = GetComponent<Renderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        oldColor = glow.material.GetColor("_TintColor");
        if (!goBackward)
            glow.material.SetColor("_TintColor", new Color(oldColor.r, oldColor.g, oldColor.b, Mathf.MoveTowards(oldColor.a, 1f, Time.deltaTime)));
        if (goBackward)
            glow.material.SetColor("_TintColor", new Color(oldColor.r, oldColor.g, oldColor.b, Mathf.MoveTowards(oldColor.a, 0.3f, Time.deltaTime)));


        if (glow.material.GetColor("_TintColor").a == 1f)
            goBackward = true;
        if (glow.material.GetColor("_TintColor").a == 0.3f)
            goBackward = false;

    }
}
