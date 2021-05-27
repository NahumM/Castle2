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
    [SerializeField] List<Warrior> warriors = new List<Warrior>();
    [SerializeField] List<Vector3> movingPath;
    [SerializeField] bool startMoving;
    [SerializeField] int warriorsInArmy;
    [SerializeField] Army armyToAttack;


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
        if (!inTheBattle)
        {
            if (armyBelongs == CastleBehaviour.Belongs.Player) Debug.Log("AttachOtherArmy called : Army - " + army.gameObject.name);
            armyToAttack = army;
            army.armyToAttack = this;
            inTheBattle = true;
            army.inTheBattle = true;
            int lessWarriors = Mathf.Min(warriors.Count, army.warriors.Count);
            for (int i = 0; i < lessWarriors; i++)
            {
                Vector3 middlePoint = (army.warriors[i].transform.position + warriors[i].transform.position) / 2;
                army.warriors[i].StopWarrior(middlePoint);
                warriors[i].AttackWarrior(army.warriors[i], middlePoint);
            }
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
            if (inTheBattle) armyToAttack.WontheBattle();
            if (attackingCastle)
            {
                castleToAttack.CaptureCastle(armyBelongs);
                if (castleToAttack.currentArmy == null)
                    castleToAttack.currentArmy = this;
            }
            Destroy(this.gameObject);
        }
        billboard.SetValue(warriorsInArmy);
    }

    void WarriorsToAttack(CastleBehaviour castle)
    {
        foreach (Warrior war in warriors)
        {
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
            }
        }
        //else startMoving = false;
    }
    void Update()
    {
        if (warriors.Count > 0)
            billboard.gameObject.transform.position = new Vector3 (warriors[0].transform.position.x, billboard.gameObject.transform.position.y, warriors[0].transform.position.z);
        FollowThePath();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isEnemyArmy && !inTheBattle)
        {
            if (other.CompareTag("PlayerCastle"))
            {
                other.GetComponent<CastleBehaviour>().TakeWarrioursFromCastle(this);
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

    public void WontheBattle()
    {
        inTheBattle = false;
        foreach (Warrior war in warriors)
        {
            war.attacking = false;
            war.agent.enabled = true;
        }
    }
}
