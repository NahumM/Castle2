using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    CastleBehaviour castleToAttack;


    bool isTrainingPassed;
    [SerializeField] bool tutorial;
    [SerializeField] GameObject hand;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] List<string> tutorialTextes = new List<string>();
    Animator anim;


    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (tutorial && !isTrainingPassed)
        {
            anim = hand.GetComponent<Animator>();
            StartCoroutine("TutorialStart");
        }
    }

    IEnumerator TutorialStart()
    {
        yield return new WaitForSeconds(1f);
        text.text = tutorialTextes[0];
        hand.SetActive(true);
        text.gameObject.SetActive(true);
    }

    void TutorialPoint(int i)
    {
        if (i == 2)
        {
            hand.SetActive(false);
            text.gameObject.SetActive(false);
            tutorial = false;
            return;
        }
        text.text = tutorialTextes[i];
        hand.SetActive(true);
        text.gameObject.SetActive(true);
        anim.SetInteger("TutorialStep", i);
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
                    if (tutorial) TutorialPoint(1);
                    activeCastle = hit.collider.GetComponent<CastleBehaviour>();
                        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, transform.parent);
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
                if (hit.collider.CompareTag("Forest"))
                {
                    drawing = false;
                    movingPoints.Clear();
                    Destroy(currentLine);
                    return;
                }
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
                        {
                            castleToAttack = hit.collider.GetComponent<CastleBehaviour>();
                            castleToAttack.currentLine = currentLine;
                            mouseClickPoint = hit.collider.transform.position;
                            mouseClickPoint.y = 0.03f;
                            movingPoints.Add(mouseClickPoint);
                            lineRenderer.positionCount++;
                            lineRenderer.SetPosition(lineRenderer.positionCount - 1, mouseClickPoint);
                            if (tutorial) TutorialPoint(2);
                            if (!hit.collider.CompareTag("PlayerCastle"))
                                activeCastle.MoveArmyToAttack(movingPoints, false);
                            else activeCastle.MoveArmyToAttack(movingPoints, true);
                            currentLine = null;
                        }
                    }
                    else
                    {
                        if (tutorial) TutorialPoint(0);
                        Destroy(currentLine);
                    }

                }
                else
                {
                    if (tutorial) TutorialPoint(0);
                    Destroy(currentLine);
                }
                drawing = false;
                activeCastle = null;
                movingPoints.Clear();
            }
        }
    }
}
