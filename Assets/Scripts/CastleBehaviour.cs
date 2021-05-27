using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleBehaviour : MonoBehaviour
{

    GameObject Zone;
    [SerializeField] GameObject torus;
    [SerializeField] GameObject armyPrefab;
    public Transform door;

    [HideInInspector] public int warriorsReady;
    bool underAttack;

    [HideInInspector] public Army currentArmy;


    [HideInInspector] public int warsJumpedinHole;
    bool startCapturing;
    public Transform jumpPosition;
    bool holeBloping;
    [SerializeField] GameObject hole;
    float target = 1.2f;
    float holeScaleValue = 3.6f;
    float zoneScaleValue;
    float zoneTarget = 0f;
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

    void HoleBlop()
    {
        holeBloping = true;
    }
    public void ChangeCastleBelongs(Belongs castleBelong)
    {
        switch (castleBelong)
        {
            case Belongs.Enemy:
                this.gameObject.tag = "EnemyCastle";
                torus.GetComponent<Renderer>().material = redMat;
               // Zone.GetComponent<Renderer>().material.color = redZone;
                isEnemyCastle = true;
                break;
            case Belongs.Player:
                this.gameObject.tag = "PlayerCastle";
                torus.GetComponent<Renderer>().material = blueMat;
                //Zone.GetComponent<Renderer>().material.color = blueZone;
                isEnemyCastle = false;
                break;
            case Belongs.Empty:
                this.gameObject.tag = "EmptyCastle";
                break;
        }
        castleBelongs = castleBelong;
        underAttack = false;
    }

    IEnumerator WarriorsCounter(bool once)
    {
        if (!once) yield return new WaitForSeconds(spawnRate - 0.1f);
        HoleBlop();
        yield return new WaitForSeconds(0.1f);
        if (warriorsReady != 0)
        {
            if (warriorsReady < maximumWarriors && !underAttack && !startCapturing)
            {
                warriorsReady++;
            }
            if (currentArmy == null)
            {
                List<Vector3> positionOfCastle = new List<Vector3>();
                positionOfCastle.Add(transform.position);
                CreateArmyToAttack(positionOfCastle);
                currentArmy.AddWarriorsToArmy(1, jumpPosition.position);
            }
            else currentArmy.AddWarriorsToArmy(1, jumpPosition.position);
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
            positionOfCastle.Add(closestCastlePosition);
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
    }

    public void MoveArmyToAttack(List<Vector3> movePositions)
    {
        List<Vector3> movingPoints = new List<Vector3>(movePositions);
        if (currentArmy != null)
        {
            currentArmy.MoveArmyToPath(movingPoints);
            currentArmy = null;
        }
    }

    public void CreateArmyToAttack(List<Vector3> movePositions)
    {
        List<Vector3> movingPoints = new List<Vector3>(movePositions);
        var army = Instantiate(armyPrefab, transform.position, Quaternion.identity);
        currentArmy = army.GetComponent<Army>();
        if (isEnemyCastle) currentArmy.isEnemyArmy = true;
        currentArmy.armyBelongs = castleBelongs;
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
