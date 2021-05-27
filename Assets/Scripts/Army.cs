using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Army : MonoBehaviour
{
    public bool isEnemyArmy;
    [SerializeField] bool inTheBattle;
    public GameObject warriorPrefab;
    public float armySpeed;
    Billboard billboard;

    [Header("Debug Stats:")]
    [SerializeField] CastleBehaviour castleToAttack;
    [SerializeField] bool attackingCastle;
    public CastleBehaviour.Belongs armyBelongs;
    public List<Warrior> warriors = new List<Warrior>();
    [SerializeField] List<Vector3> movingPath;
    [SerializeField] bool startMoving;
    [SerializeField] int warriorsInArmy;
    public Army armyToAttack;
    List<Army> armiesToAttack = new List<Army>();


    private void Awake()
    {
        billboard = transform.GetChild(0).GetComponent<Billboard>();
    }

    private void Start()
    {
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
            army.armyToAttack = this;
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
        startMoving = true;

    }

    public void JumpToCastle(CastleBehaviour castle)
    {
        attackingCastle = true;
        foreach (Warrior war in warriors)
        {
            war.StartJump(false, castle.jumpPosition.position);
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
                armyToAttack.WontheBattle(this);
            }
            if (attackingCastle)
            {
                castleToAttack.CaptureCastle(armyBelongs);
                if (castleToAttack.currentArmy == null)
                    castleToAttack.currentArmy = this;
            }
            Destroy(this);
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
        if (inTheBattle && armyToAttack == null) WontheBattle(null);
        if (startMoving && movingPath.Count > 0 && !inTheBattle && !attackingCastle)
        {
            transform.position = Vector3.MoveTowards(transform.position, movingPath[0], Time.deltaTime * armySpeed);
            if (Vector3.Distance(transform.position, movingPath[0]) < 0.1f)
            {
                movingPath.RemoveAt(0);
            }
        }
        //else startMoving = false;
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
                    castleToAttack.AttackCastle();
                    WarriorsToAttack(castleToAttack);
            }
        }
        if (isEnemyArmy && !inTheBattle)
        {
            if (other.CompareTag("PlayerCastle") || other.CompareTag("EmptyCastle"))
            {

                    castleToAttack = other.GetComponent<CastleBehaviour>();
                    castleToAttack.AttackCastle();
                    WarriorsToAttack(castleToAttack);
            }
            if (other.CompareTag("EnemyCastle"))
            {
                other.GetComponent<CastleBehaviour>().TakeWarrioursFromCastle(this);
            }
        }

        if (!isEnemyArmy)
        {
            if (other.CompareTag("EnemyArmy"))
            {
                if (!other.GetComponent<Army>().inTheBattle)
                    AttackOtherArmy(other.GetComponent<Army>());
            }
        }
    }

    public void WontheBattle(Army army)
    {
        if (armiesToAttack.Count <= 1)
        {
            inTheBattle = false;
            foreach (Warrior war in warriors)
            {
                war.attacking = false;
                war.agent.enabled = true;
            }
            if (castleToAttack != null)
                WarriorsToAttack(castleToAttack);
        }
        else
        {
            armiesToAttack.Remove(army);
            AttackOtherArmy(armiesToAttack[0]);
        }
    }

    private void OnDestroy()
    {
        Destroy(this.gameObject);
    }
}
