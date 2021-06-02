using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleBehaviour : MonoBehaviour
{

    GameObject Zone;
    [SerializeField] GameObject torus;
    [SerializeField] GameObject armyPrefab;
    [HideInInspector] public LevelManager levelManager;
    public Transform door;

    [HideInInspector] public int warriorsReady;
    public bool underAttack;

    public Army currentArmy;


    [HideInInspector] public int warsJumpedinHole;
    bool startCapturing;
    public Transform jumpPosition;
    bool holeBloping;
    [SerializeField] GameObject hole;
    float target = 1.2f;
    float holeScaleValue = 3.6f;
    public enum Belongs { Enemy, Player, Empty};
    public Belongs castleBelongs;

    [SerializeField] Material redMat;
    [SerializeField] Material blueMat;

    [Header("Castle Stats:")]
    [HideInInspector] public bool isEnemyCastle;
    [SerializeField] int startWarriors;
    [SerializeField] int spawnRate;
    [SerializeField] int maximumWarriors;
    [SerializeField] int attackRate;
    

    void Start()
    {
        Zone = transform.GetChild(1).gameObject;
        warriorsReady = startWarriors;
        StartCoroutine("WarriorsCounter", false);
        StartCoroutine("EnemyAttackRate");
        if (startWarriors > 0)
            CaptureCastle(castleBelongs);
        else ChangeCastleBelongs(castleBelongs);

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
        levelManager.CastleCapture();
        castleBelongs = castleBelong;
        underAttack = false;
    }

    IEnumerator WarriorsCounter(bool once)
    {
        if (!once) yield return new WaitForSeconds(spawnRate);
        if ((castleBelongs == Belongs.Enemy || castleBelongs == Belongs.Player) && !underAttack)
        {
            if (warriorsReady < maximumWarriors)
            {
                warriorsReady++;
            }
            if (currentArmy == null)
            {
                List<Vector3> positionOfCastle = new List<Vector3>();
                positionOfCastle.Add(transform.position + new Vector3(0, 0.06f, 0));
                CreateArmyToAttack(positionOfCastle);
                currentArmy.AddWarriorsToArmy(1, jumpPosition.position);
            }
            else if (currentArmy.warriors.Count != 0 && !underAttack)
            {
                currentArmy.AddWarriorsToArmy(1, jumpPosition.position);
            }
            else if (currentArmy.warriors.Count == 0) Destroy(currentArmy.gameObject);
        }
        if (currentArmy == null && underAttack)
        {
            yield return new WaitForSeconds(2f);
            underAttack = false;
        }
            if (!once)
        StartCoroutine("WarriorsCounter", false);
    }

    IEnumerator EnemyAttackRate()
    {
        yield return new WaitForSeconds(attackRate);
        if (castleBelongs == Belongs.Enemy)
        {
            GameObject[] oppositeCastles = GameObject.FindGameObjectsWithTag("PlayerCastle");
            GameObject[] emptyCastles = GameObject.FindGameObjectsWithTag("EmptyCastle");
            List<GameObject> allCastles = new List<GameObject>();
            allCastles.AddRange(oppositeCastles);
            allCastles.AddRange(emptyCastles);
            Vector3 closestCastlePosition = Vector3.zero;
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
            List<Vector3> positionOfCastle = new List<Vector3>();
            positionOfCastle.Add(closestCastlePosition + new Vector3(0, 0.06f, 0));
            //CreateArmyToAttack(positionOfCastle);
            MoveArmyToAttack(positionOfCastle);
        }
        StartCoroutine("EnemyAttackRate");
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

    public void MoveArmyToAttack(List<Vector3> movePositions)
    {
        List<Vector3> movingPoints = new List<Vector3>(movePositions);
        if (currentArmy != null)
        {
            currentArmy.MoveArmyToPath(movingPoints);
            currentArmy.mainCastle = null;
            currentArmy = null;
        }
    }

    public void CreateArmyToAttack(List<Vector3> movePositions)
    {
        List<Vector3> movingPoints = new List<Vector3>(movePositions);
        var army = Instantiate(armyPrefab, transform.position + new Vector3(0, 0.06f, 0), Quaternion.identity);
        currentArmy = army.GetComponent<Army>();
        currentArmy.armyBelongs = castleBelongs;
        currentArmy.mainCastle = this;
        ChangeArmyValue(warriorsReady / 2);
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
        if (startCapturing)
        {
            /* //zoneScaleValue = Mathf.MoveTowards(zoneScaleValue, zoneTarget, Time.deltaTime * 1);
             zoneScaleValue = Mathf.Lerp(zoneScaleValue, zoneTarget, Time.deltaTime * 1);

             Zone.transform.localScale = new Vector3(zoneScaleValue, zoneScaleValue, zoneScaleValue);

             if (zoneScaleValue < 0.03f)
             {
                 if (castleBelongs == Belongs.Player)
                     Zone.GetComponent<Renderer>().material.color = blueZone;
                 if (castleBelongs == Belongs.Enemy)
                     Zone.GetComponent<Renderer>().material.color = redZone;
                 for (int i = 0; i < warsJumpedinHole; i++)
                 {
                     StartCoroutine("WarriorsCounter", true);
                 }
                 warsJumpedinHole = 0;
                 zoneTarget = 0.85f;
             }

             if (zoneTarget == 0.85f && zoneScaleValue > 0.82f)
             {
                 startCapturing = false;
                 zoneTarget = 0f;
             } */

        }
    }
}
