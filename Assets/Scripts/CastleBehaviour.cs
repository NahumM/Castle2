using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleBehaviour : MonoBehaviour
{

    GameObject Zone;
    public bool gameOver;
    [SerializeField] GameObject torus;
    [SerializeField] GameObject armyPrefab;
    [HideInInspector] public LevelManager levelManager;
    [SerializeField] List<GameObject> enemyPathways = new List<GameObject>();
    public Transform door;

    public int warriorsReady;
    public bool underAttack;

    public Army currentArmy;

    public GameObject currentLine;


    [HideInInspector] public int warsJumpedinHole;
    bool startCapturing;
    public Transform jumpPosition;
    bool holeBloping;
    [SerializeField] GameObject hole;
    float target = 1.2f;
    float holeScaleValue = 3.6f;
    public float circleDistanceOfSpawn = 0.3f;
    public enum Belongs { Enemy, Player, Empty};
    public Belongs castleBelongs;

    [SerializeField] Material redMat;
    [SerializeField] Material blueMat;

    [Header("Castle Stats:")]
    [HideInInspector] public bool isEnemyCastle;
    [SerializeField] int startWarriors;
    [SerializeField] float spawnRate;
    [SerializeField] int maximumWarriors;
    [SerializeField] int attackRate;
    

    void Start()
    {
        Zone = transform.GetChild(1).gameObject;
        warriorsReady = startWarriors;
        if (startWarriors > 0)
        {
            for (int i = 0; i < startWarriors; i++)
            {
                StartCoroutine("WarriorsCounter", true);
            }
        }
        StartCoroutine("WarriorsCounter", false);
        StartCoroutine("EnemyAttackRate");
        if (startWarriors > 0)
            CaptureCastle(castleBelongs);
        else ChangeCastleBelongs(castleBelongs);
    }

    public void CurrentLine(GameObject line)
    {
        currentLine = line;
    }

    public void ChangeCastleBelongs(Belongs castleBelong)
    {
        switch (castleBelong)
        {
            case Belongs.Enemy:
                this.gameObject.tag = "EnemyCastle";
                torus.GetComponent<Renderer>().material = redMat;
                this.castleBelongs = castleBelong;
                isEnemyCastle = true;
                break;
            case Belongs.Player:
                this.gameObject.tag = "PlayerCastle";
                torus.GetComponent<Renderer>().material = blueMat;
                //Zone.GetComponent<Renderer>().material.color = blueZone;
                this.castleBelongs = castleBelong;
                isEnemyCastle = false;
                break;
            case Belongs.Empty:
                this.gameObject.tag = "EmptyCastle";
                this.castleBelongs = castleBelong;
                break;
        }
        if (levelManager != null)
            levelManager.CastleCapture();
        castleBelongs = castleBelong;
        if (currentLine != null && castleBelong == Belongs.Player)
            Destroy(currentLine);
        underAttack = false;
    }


    IEnumerator WarriorsCounter(bool once)
    {
        if (!gameOver)
        {
            if (!once) yield return new WaitForSeconds(spawnRate);
            if ((castleBelongs == Belongs.Enemy || castleBelongs == Belongs.Player) && !underAttack)
            {
                    if (warriorsReady <= maximumWarriors || once)
                    {
                        if (currentArmy == null)
                        {
                            warriorsReady = 0;
                            CreateArmy();
                        }
                        else if (currentArmy.warriors.Count != 0 && !underAttack)
                        {
                            currentArmy.AddWarriorsToArmy(1, jumpPosition.position, true);
                        }
                        else if (currentArmy.warriors.Count == 0) Destroy(currentArmy.gameObject);
                        warriorsReady++;
                    }
                //if (currentArmy == null || !once) warriorsReady = 0;
            }
            if (currentArmy == null && underAttack)
            {
                //underAttack = false;
                warriorsReady = 0;
            }
            if (!once)
            {
                StartCoroutine("WarriorsCounter", false);
            }
        }
    }

    void CreateArmy()
    {
        List<Vector3> positionOfCastle = new List<Vector3>();
        positionOfCastle.Add(transform.position + new Vector3(0, 0.06f, 0));
        CreateArmyToAttack(positionOfCastle);
        currentArmy.AddWarriorsToArmy(1, jumpPosition.position, true);
    }

    IEnumerator EnemyAttackRate()
    {
        if (!gameOver)
        {
            yield return new WaitForSeconds(attackRate);
            if (castleBelongs == Belongs.Enemy)
            {
                Vector3 closestCastlePosition = Vector3.zero;
                List<Vector3> positionOfCastle = new List<Vector3>();
                if (enemyPathways.Count < 1)
                {
                    GameObject[] oppositeCastles = GameObject.FindGameObjectsWithTag("PlayerCastle");
                    GameObject[] emptyCastles = GameObject.FindGameObjectsWithTag("EmptyCastle");
                    List<GameObject> allCastles = new List<GameObject>();
                    allCastles.AddRange(oppositeCastles);
                    allCastles.AddRange(emptyCastles);
                    float distanceToCastle = 999f;
                    if (oppositeCastles.Length <= 0) yield break;
                    foreach (GameObject castle in allCastles)
                    {
                        if (Vector3.Distance(transform.position, castle.transform.position) < distanceToCastle)
                        {
                            distanceToCastle = Vector3.Distance(transform.position, castle.transform.position);
                            closestCastlePosition = castle.transform.position;
                        }
                    }
                    positionOfCastle.Add(closestCastlePosition + new Vector3(0, 0.06f, 0));
                } else
                {
                    foreach (Transform child in enemyPathways[Random.Range(0, enemyPathways.Count)].transform)
                    {
                        positionOfCastle.Add(child.position);
                    }
                }
                MoveArmyToAttack(positionOfCastle, null);
            }
            StartCoroutine("EnemyAttackRate");
        }
    }

    public void ChangeArmyValue(int value)
    {
        if (value <= maximumWarriors)
        {
            warriorsReady = value;
        } else
        {
            warriorsReady = maximumWarriors;
        }
    }

    public void GameOver()
    {
        gameOver = true;
        StopAllCoroutines();
    }

    public void TakeWarrioursFromCastle(Army army)
    {
        //ChangeArmyValue(warriorsReady / 2);
        //army.AddWarriorsToArmy(warriorsReady);

    }

    public void AttackCastle()
    {
        underAttack = true;
    }

    public void CaptureCastle(Belongs castleBelong)
    {
        //zoneScaleValue = Zone.transform.localScale.x;
        ChangeCastleBelongs(castleBelong);
        startCapturing = true;
        for (int i = 0; i < warsJumpedinHole; i++)
        {
            StartCoroutine("WarriorsCounter", true);
        }
        warsJumpedinHole = 0;
    }

    public void MoveArmyToAttack(List<Vector3> movePositions, CastleBehaviour castleToAttack)
    {
        List<Vector3> movingPoints = new List<Vector3>(movePositions);
        if (currentArmy != null)
        {
            if (castleToAttack != null) currentArmy.goingToAllies = castleToAttack;
            currentArmy.MoveArmyToPath(movingPoints);
            currentArmy.mainCastle = null;
            currentArmy.currentLine = currentLine;
            currentArmy = null;
            warriorsReady = 0;
        }
    }

    public void CreateArmyToAttack(List<Vector3> movePositions)
    {
        List<Vector3> movingPoints = new List<Vector3>(movePositions);
        var army = Instantiate(armyPrefab, transform.position + new Vector3(0, 0.06f, 0), Quaternion.identity, transform.parent);
        currentArmy = army.GetComponent<Army>();
        currentArmy.armyBelongs = castleBelongs;
        currentArmy.mainCastle = this;
        currentArmy.circleDistance = circleDistanceOfSpawn;
        ChangeArmyValue(1);
        //currentArmy.AddWarriorsToArmy(warriorsReady);
        currentArmy.MoveArmyToPath(movingPoints);
    }

    private void Update()
    {
        if (holeBloping)
        {
            holeScaleValue = Mathf.MoveTowards(holeScaleValue, target, Time.deltaTime * 10);

            hole.transform.localScale = new Vector3(holeScaleValue, holeScaleValue, holeScaleValue);

            if (holeScaleValue == 1.2f)
                target = 3.6f;

            if (target == 3.6f && holeScaleValue == 3.6f)
            {
                holeBloping = false;
                target = 1.2f;
            }
        }
    }
}
