using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Army : MonoBehaviour
{

    static int id;
    public bool isEnemyArmy;
    public bool inTheBattle;
    public GameObject warriorPrefab;
    public float armySpeed;
    Billboard billboard;
    public int startArmyAmount;
    public bool goingToAllies;

    int counter;

    List<Vector3> positionsOfSpawn;

    [Header("Debug Stats:")]
    [SerializeField] CastleBehaviour castleToAttack;
    [SerializeField] bool attackingCastle;
    public CastleBehaviour mainCastle;
    public CastleBehaviour.Belongs armyBelongs;
    public List<Warrior> warriors = new List<Warrior>();
    [SerializeField] List<Vector3> movingPath;
    public bool startMoving;
    [SerializeField] int warriorsInArmy;
    public Army armyToAttack;
    public List<Army> armiesToAttack = new List<Army>();


    private void Awake()
    {
        billboard = transform.GetChild(0).GetComponent<Billboard>();
        positionsOfSpawn = GetCirclePositionsOfWarriors(new Vector3(transform.position.x, transform.position.z, transform.position.y), 0.3f, 20);
    }

    private void Start()
    {
        gameObject.name = "Army(" + id + ")";
        id++;
        CheckBelongs();
    }

    void CheckBelongs()
    {
        switch (armyBelongs)
        {
            case CastleBehaviour.Belongs.Enemy:
                gameObject.tag = "EnemyArmy";
                break;
            case CastleBehaviour.Belongs.Player:
                gameObject.tag = "PlayerArmy";
                break;
            case CastleBehaviour.Belongs.Empty:
                gameObject.tag = "EmptyArmy";
                AddWarriorsToArmy(startArmyAmount, transform.position, true);
                break;
        }
    }

    public void AddWarriorsToArmy(int amount, Vector3 jumpPoint, bool jump)
    {
        for (int i = 0; i < amount; i++)
        {
            var warrior = Instantiate(warriorPrefab, jumpPoint, Quaternion.identity, transform.parent);
            Warrior war = warrior.GetComponent<Warrior>();
            war.warriorBelongs = armyBelongs;
            war.army = this;
            if (!jump) war.jump = false;
            war.positionToJump = positionsOfSpawn[counter];
            counter++;
            warriors.Add(war);
            warriorsInArmy++;
            billboard.SetValue(warriorsInArmy);
        }
    }

    public void AttackOtherArmy(Army army)
    {
            armyToAttack = army;
            if (!armiesToAttack.Contains(army))
            armiesToAttack.Add(army);
            if (!army.armiesToAttack.Contains(this))
            army.armiesToAttack.Add(this);
            inTheBattle = true;
            army.inTheBattle = true;
            List<Warrior> attackers = new List<Warrior>();
            List<Warrior> defenders = new List<Warrior>();
            foreach (Warrior war in warriors)
            {
                if (!war.inDuel)
                    attackers.Add(war);
            }
            foreach (Warrior war in army.warriors)
            {
                if (!war.inDuel)
                    defenders.Add(war);
            }
            int lessWarriors = Mathf.Min(attackers.Count, defenders.Count);
            for (int i = 0; i < lessWarriors; i++)
            {
                Vector3 middlePoint = (defenders[i].transform.position + attackers[i].transform.position) / 2;
                defenders[i].StopWarrior(middlePoint);
                attackers[i].AttackWarrior(defenders[i], middlePoint);
            }
    }

    public void MoveArmyToPath(List<Vector3> path)
    {
        movingPath = path;
        foreach (Warrior war in warriors)
        {
            var lookPos = movingPath[0] - transform.position;
            lookPos.y = 0.06f;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            
            war.gameObject.transform.rotation = rotation;
        }
        startMoving = true;

    }

    public void JumpToCastle(CastleBehaviour castle)
    {
        attackingCastle = true;
        foreach (Warrior war in warriors)
        {
            war.castleToAttack = castleToAttack;
            war.goingInsideCastle = true;
            war.StartJump(false, castle.jumpPosition.position, true);
        }
    }

    public void RemoveWarriorFromArmy(Warrior war)
    {
        warriors.Remove(war);
        warriorsInArmy--;
        if (warriorsInArmy < 1)
        {
            if (inTheBattle)
            {
                foreach (Army arm in armiesToAttack)
                {
                    arm.WontheBattle(this);
                }
            }
            if (mainCastle != null)
            {
                if (mainCastle.currentArmy == this)
                    mainCastle.warriorsReady--;
            }
            if (attackingCastle)
            {
                castleToAttack.CaptureCastle(armyBelongs);
                if (castleToAttack.currentArmy == null)
                    castleToAttack.currentArmy = this;
            }
            Destroy(gameObject);
            return;
        }
        billboard.SetValue(warriorsInArmy);
    }

    void WarriorsToAttack(CastleBehaviour castle)
    {
        foreach (Warrior war in warriors)
        {
            if (!war.inDuel)
             war.AttackCastle(castle);
        }
    }

    void FollowThePath()
    {
        if (startMoving && movingPath.Count > 0 && !inTheBattle && !attackingCastle)
        {
            transform.position = Vector3.MoveTowards(transform.position, movingPath[0], Time.deltaTime * armySpeed);
            if (Vector3.Distance(transform.position, movingPath[0]) < 0.1f)
            {
                movingPath.RemoveAt(0);
                if (movingPath.Count > 0)
                {
                    foreach (Warrior war in warriors)
                    {
                        var lookPos = movingPath[0] - transform.position;
                        lookPos.y = 0.06f;
                        Quaternion rotation = Quaternion.LookRotation(lookPos);

                        war.gameObject.transform.rotation = rotation;
                    }
                }
            }
        }
        else startMoving = false;
    }
    void Update()
    {
        if (warriors.Count > 0)
            billboard.gameObject.transform.position = new Vector3 (warriors[0].transform.position.x, warriors[0].transform.position.y + 1f, warriors[0].transform.position.z);
        FollowThePath();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (armyBelongs == CastleBehaviour.Belongs.Player && !inTheBattle)
        {
            if (other.CompareTag("PlayerCastle"))
            {
                //other.GetComponent<CastleBehaviour>().TakeWarrioursFromCastle(this);
            }
            if (other.CompareTag("EnemyCastle") || other.CompareTag("EmptyCastle"))
            {
                    castleToAttack = other.GetComponent<CastleBehaviour>();
                if (!castleToAttack.underAttack && castleToAttack.currentArmy == null)
                {
                    castleToAttack.AttackCastle();
                    JumpToCastle(castleToAttack);
                }
                if (!castleToAttack.underAttack && castleToAttack.currentArmy != null)
                {
                    castleToAttack.AttackCastle();
                    AttackOtherArmy(castleToAttack.currentArmy);
                }
            }
        }
        if (armyBelongs == CastleBehaviour.Belongs.Enemy && !inTheBattle)
        {
            if (other.CompareTag("PlayerCastle") || other.CompareTag("EmptyCastle"))
            {
                castleToAttack = other.GetComponent<CastleBehaviour>();
                if (castleToAttack.currentArmy == null)
                {
                    castleToAttack.AttackCastle();
                    JumpToCastle(castleToAttack);
                }
                if (!castleToAttack.underAttack && castleToAttack.currentArmy != null)
                {
                    castleToAttack.AttackCastle();
                    castleToAttack.currentArmy.AttackOtherArmy(this);
                }
            }
            if (other.CompareTag("EnemyCastle"))
            {
                
            }
        }

        if (armyBelongs == CastleBehaviour.Belongs.Player)
        {
            if (other.CompareTag("EnemyArmy"))
            {
                if (!other.GetComponent<Army>().inTheBattle)
                    AttackOtherArmy(other.GetComponent<Army>());
            }
            if (other.CompareTag("PlayerArmy") || other.CompareTag("EmptyArmy"))
            {
                var alyArmy = other.GetComponent<Army>();
                if (!alyArmy.inTheBattle && !alyArmy.startMoving && !alyArmy.attackingCastle)
                {
                    foreach (Warrior war in alyArmy.warriors)
                    {
                        positionsOfSpawn = GetCirclePositionsOfWarriors(new Vector3(transform.position.x, transform.position.z, transform.position.y), 0.3f, 20);
                        AddWarriorsToArmy(1, war.transform.position, false);
                        Destroy(war.gameObject);
                    }
                    if (goingToAllies)
                     alyArmy.mainCastle.currentArmy = this;
                    Destroy(alyArmy.gameObject);
                }
            }
        }

        if (armyBelongs == CastleBehaviour.Belongs.Enemy)
        {
            if (other.CompareTag("EnemyArmy") || other.CompareTag("EmptyArmy"))
            {
                var alyArmy = other.GetComponent<Army>();
                if (!alyArmy.inTheBattle && !alyArmy.startMoving && !alyArmy.attackingCastle)
                {
                    foreach (Warrior war in alyArmy.warriors)
                    {
                        positionsOfSpawn = GetCirclePositionsOfWarriors(new Vector3(transform.position.x, transform.position.z, transform.position.y), 0.3f, 20);
                        AddWarriorsToArmy(1, war.transform.position, true);
                        Destroy(war.gameObject);
                    }
                    Destroy(alyArmy.gameObject);
                }
            }
        }
    }

    public void WontheBattle(Army army)
    {
        ClearListFromEmpty(armiesToAttack);
        if (warriorsInArmy > 0)
        {
            if (armiesToAttack.Count <= 1)
            {
                inTheBattle = false;

                foreach (Warrior war in warriors)
                {
                    war.attacking = false;
                    //  war.agent.enabled = true;
                }
                if (castleToAttack != null)
                    WarriorsToAttack(castleToAttack);
                if (mainCastle != null)
                {
                    if (mainCastle.currentArmy == this)
                        mainCastle.underAttack = false;
                }
                StartCoroutine("WinningDelay");
            }
            else
            {
                armiesToAttack.Remove(army);
                if (armiesToAttack[0] != null)
                    AttackOtherArmy(armiesToAttack[0]);
            }
        } 
    }

    void ClearListFromEmpty(List<Army> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
                list.RemoveAt(i);
        }
    }

    List<Vector3> GetCirclePositionsOfWarriors(Vector3 centerPosition, float distance, int positionCount)
    {
        List<Vector3> positionList = new List<Vector3>();
        for (int i = 0; i < positionCount; i++)
        {
            float angle = i * (360f / positionCount);
            Vector3 dir = ApplyRotationToVector(new Vector3(1, 0), angle);
            Vector3 position = centerPosition + dir * distance;
            positionList.Add(position);
        }
        return positionList;
    }

    Vector3 ApplyRotationToVector(Vector3 vector, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vector;
    }

    IEnumerator WinningDelay()
    {
        yield return new WaitForSeconds(0.1f);
        startMoving = true;
    }
}
