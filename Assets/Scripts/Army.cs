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

    [Header("Debug Stats:")]
    [SerializeField] CastleBehaviour castleToAttack;
    [SerializeField] bool attackingCastle;
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
    }

    private void Start()
    {
        gameObject.name = "Army(" + id + ")";
        id++;
        if (isEnemyArmy) gameObject.tag = "EnemyArmy"; else gameObject.tag = "PlayerArmy";
    }

    public void AddWarriorsToArmy(int amount, Vector3 jumpPoint)
    {
        for (int i = 0; i < amount; i++)
        {
            var warrior = Instantiate(warriorPrefab, jumpPoint, Quaternion.identity);
            Warrior war = warrior.GetComponent<Warrior>();
            if (isEnemyArmy) war.isEnemy = true;
            war.army = this;
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
            war.StartJump(false, castle.jumpPosition.position);
        }
    }

    public void RemoveWarriorFromArmy(Warrior war)
    {
        warriors.Remove(war);
        warriorsInArmy--;
        if (warriorsInArmy < 0) Debug.Log("Warriors In Army - " + warriorsInArmy + " name: " + gameObject.name);
        if (warriorsInArmy < 1)
        {
            if (inTheBattle)
            {
                foreach (Army arm in armiesToAttack)
                {
                    arm.WontheBattle(this);
                }
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
        if (!isEnemyArmy && !inTheBattle)
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
        if (isEnemyArmy && !inTheBattle)
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

        if (!isEnemyArmy)
        {
            if (other.CompareTag("EnemyArmy"))
            {
                if (!other.GetComponent<Army>().inTheBattle)
                    AttackOtherArmy(other.GetComponent<Army>());
            }
            if (other.CompareTag("PlayerArmy"))
            {
                var alyArmy = other.GetComponent<Army>();
                if (!alyArmy.inTheBattle && !alyArmy.startMoving && !alyArmy.attackingCastle)
                {
                    foreach (Warrior war in alyArmy.warriors)
                    {
                        AddWarriorsToArmy(1, war.transform.position);
                        Destroy(war.gameObject);
                    }
                    Destroy(alyArmy.gameObject);
                }
            }
        }

        if (isEnemyArmy)
        {
            if (other.CompareTag("EnemyArmy"))
            {
                var alyArmy = other.GetComponent<Army>();
                if (!alyArmy.inTheBattle && !alyArmy.startMoving && !alyArmy.attackingCastle)
                {
                    foreach (Warrior war in alyArmy.warriors)
                    {
                        AddWarriorsToArmy(1, war.transform.position);
                        Destroy(war.gameObject);
                    }
                    Destroy(alyArmy.gameObject);
                }
            }
        }
    }

    public void WontheBattle(Army army)
    {
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
                //armiesToAttack.Clear();
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

    IEnumerator WinningDelay()
    {
        yield return new WaitForSeconds(1f);
        foreach (Warrior war in warriors)
        {
            if (!war.dying)
                war.anim.SetBool("isWinning", true);
        }
        yield return new WaitForSeconds(1f);
        foreach (Warrior war in warriors)
        {
            war.anim.SetBool("isWinning", false);
        }
        startMoving = true;
    }
}
