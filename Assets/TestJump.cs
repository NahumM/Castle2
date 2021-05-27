using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJump : MonoBehaviour
{
    Vector3 startPos;
    Vector3 endPos;
    public Transform target;
    public float trajectoryHeight = 5;
    public float speed;

    private void Start()
    {
        startPos = transform.position;
        endPos = target.position;
    }

    void Update()
    {
        float cTime = Time.time * speed;
        Vector3 currentPos = Vector3.Lerp(startPos, endPos, cTime);
        currentPos.y += trajectoryHeight * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);
        transform.position = currentPos;
    }
}
