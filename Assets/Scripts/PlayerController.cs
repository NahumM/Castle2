using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public bool isGameEnded;
    Camera mainCamera;
    public List<Vector3> movingPoints = new List<Vector3>();
    Vector3 mouseClickPoint;

    GameObject currentLine;
    public GameObject linePrefab;
    LineRenderer lineRenderer;
    List<Vector2> mousePositions;
    bool drawing;

    CastleBehaviour activeCastle;


    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isGameEnded)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Castles")))
            {
                if (hit.collider.CompareTag("PlayerCastle"))
                {
                    activeCastle = hit.collider.GetComponent<CastleBehaviour>();
                        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
                    activeCastle.currentLine = currentLine;
                        lineRenderer = currentLine.GetComponent<LineRenderer>();
                    mouseClickPoint = hit.collider.transform.position;
                        //mouseClickPoint.x = hit.point.x;
                        //mouseClickPoint.z = hit.point.z;
                        mouseClickPoint.y = 0.06f;
                        lineRenderer.SetPosition(0, mouseClickPoint);
                        lineRenderer.SetPosition(1, mouseClickPoint);
                        drawing = true;
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = 1 << 6;
            layerMask = ~layerMask;
            if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Floor")))
            {
                if (drawing)
                {
                    if (Vector3.Distance(hit.point, mouseClickPoint) > 0.2f)
                    {
                        mouseClickPoint.x = hit.point.x;
                        mouseClickPoint.z = hit.point.z;
                        mouseClickPoint.y = 0.06f;
                        movingPoints.Add(mouseClickPoint);
                        lineRenderer.positionCount++;
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mouseClickPoint);
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (drawing)
            {
                Destroy(currentLine, movingPoints.Count * 0.3f);
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Castles")))
                {
                    if (hit.collider.CompareTag("EnemyCastle") || hit.collider.CompareTag("EmptyCastle") || hit.collider.CompareTag("PlayerCastle"))
                    {
                        if (!activeCastle.underAttack && activeCastle.currentArmy != null)
                        mouseClickPoint = hit.collider.transform.position;
                        mouseClickPoint.y = 0.03f;
                        movingPoints.Add(mouseClickPoint);
                        lineRenderer.positionCount++;
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mouseClickPoint);
                        if (!hit.collider.CompareTag("PlayerCastle"))
                        activeCastle.MoveArmyToAttack(movingPoints, false);
                        else activeCastle.MoveArmyToAttack(movingPoints, true);
                        currentLine = null;
                    }
                    else Destroy(currentLine);

                }
                else Destroy(currentLine);
                drawing = false;
                activeCastle = null;
                movingPoints.Clear();
            }
        }
    }
}
