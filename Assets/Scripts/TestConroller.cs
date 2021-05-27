using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConroller : MonoBehaviour
{
    public Army testArmy;
    Camera mainCamera;
    public List<Vector3> movingPoints = new List<Vector3>();
    Vector3 mouseClickPoint;

    GameObject currentLine;
    public GameObject linePrefab;
    LineRenderer lineRenderer;
    List<Vector2> mousePositions;
    bool drawing;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Floor"))
                {
                    currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
                    lineRenderer = currentLine.GetComponent<LineRenderer>();
                    mouseClickPoint = hit.point;
                    lineRenderer.SetPosition(0, mouseClickPoint);
                    lineRenderer.SetPosition(1, mouseClickPoint);
                    drawing = true;
                }
               // testArmy.MoveArmyTo(hit.point);
            }
        }
        if (Input.GetMouseButton(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Floor") && drawing)
                {
                    if (Vector3.Distance(hit.point, mouseClickPoint) > 0.1f)
                    {
                        mouseClickPoint.x = hit.point.x;
                        mouseClickPoint.z = hit.point.z;
                        mouseClickPoint.y = 0.03f;
                        movingPoints.Add(mouseClickPoint);
                        lineRenderer.positionCount++;
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mouseClickPoint);
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0))  
        {
            testArmy.MoveArmyToPath(movingPoints);
            //Destroy(currentLine);
        }
    }
}
