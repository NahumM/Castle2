using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Warrior : MonoBehaviour
{
    [SerializeField] GameObject warriorZone;
    [SerializeField] ParticleSystem blood;
    //Stats
    public bool isEnemy;
    //
    public bool inDuel;
    public Warrior warToAttack;
    public Army army;
    // [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Animator anim;
    Renderer render;
    [HideInInspector] public bool attacking;
    public bool goingInsideCastle;
    bool isDead;
    public bool jumpToCastle;
    public CastleBehaviour castleToAttack;
    public CastleBehaviour.Belongs warriorBelongs;
    static int id;
    Vector3 destanation;

    public float MoveTowardsSpeed;

    Color blueColor = new Color(0.1830188f, 0.5300552f, 1f, 1f);


    //JumpStats
    Vector3 startPos;
    Vector3 endPos;
    Vector3 preset;
    public float trajectoryHeight = 5;
    public float jumpSpeed;
    bool startJumping;
    float cTime;
    [HideInInspector] public bool dying;
    //
    void Start()
    {
        gameObject.name = "Warrior(" + id + ")";
        id++;
        //agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        render = transform.GetChild(1).GetComponent<Renderer>();
        CheckBelongs();
        StartJump(true, Vector3.zero);
    }

    public void StartJump(bool fromCastle, Vector3 positionToJump)
    {
        if (!inDuel)
        {
           // agent.enabled = false;
            startPos = transform.position;
            if (fromCastle)
            {
                Vector3 randomPoint = transform.position + Random.onUnitSphere * 0.2f;
                endPos = new Vector3(randomPoint.x, 0.06f, randomPoint.z);
                preset = new Vector3(endPos.x - transform.position.x, 0, endPos.z - transform.position.z);
            }
            else
            {
                goingInsideCastle = true;
                endPos = positionToJump;
                jumpToCastle = true;
            }
            Vector3 posToLook = new Vector3(endPos.x, transform.position.y, endPos.z);
            transform.LookAt(posToLook);
            startJumping = true;
        }
    }

    public void CheckBelongs()
    {
        switch (warriorBelongs)
        {
            case CastleBehaviour.Belongs.Enemy:
                render.material.color = Color.red;
                warriorZone.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.3f);
                gameObject.tag = "EnemyWarrior";
                break;
            case CastleBehaviour.Belongs.Player:
                render.material.color = blueColor;
                warriorZone.GetComponent<SpriteRenderer>().color = new Color(0.1830188f, 0.5300552f, 1f, 0.3f);
                gameObject.tag = "PlayerWarrior";
                break;
            case CastleBehaviour.Belongs.Empty:
                render.material.color = Color.gray;
                warriorZone.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                gameObject.tag = "PlayerWarrior";
                break;
        }
    }

    public void StopWarrior(Vector3 positionOfStop)
    {
        inDuel = true;
        attacking = true;
        destanation = positionOfStop;
    }

    public void AttackWarrior(Warrior war, Vector3 position)
    {
        warToAttack = war;
        attacking = true;
        inDuel = true;
        var lookPos = position - transform.position;
        lookPos.y = 0.06f;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = rotation;
        destanation = position;
    }

    public void MoveToPosition(Vector3 destanation)
    {
        //agent.destination = destanation;
        this.destanation = destanation;
    }

    public void AttackCastle(CastleBehaviour castle)
    {
        castleToAttack = castle;
        //agent.enabled = true;
        //agent.destination = castleToAttack.transform.position;
        destanation = castleToAttack.transform.position;
        attacking = true;
        // StartCoroutine("FightChecker", castle);
    }

    private void Update()
    {
            if (army.startMoving)
            {
            anim.SetBool("isRunning", true);
            }
            else anim.SetBool("isRunning", false);


        if (!inDuel && !dying)
           destanation = army.transform.position + preset;
        transform.position = Vector3.MoveTowards(transform.position, destanation, Time.deltaTime * MoveTowardsSpeed);
        if (inDuel && !dying)
        {
            if (Vector3.Distance(transform.position, destanation) < 0.3f && warriorBelongs == CastleBehaviour.Belongs.Player)
            {
                    dying = true;
                destanation = transform.position;
                warToAttack.Death();
                Death();
                inDuel = false;
            }
        }

        if (startJumping && !inDuel)
        {
            cTime += jumpSpeed * Time.deltaTime;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, cTime);
            currentPos.y += trajectoryHeight * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);
            transform.position = currentPos;
            if (Vector3.Distance(transform.position, endPos) < 0.01f)
            {
                if (!jumpToCastle)
                {
                   // agent.enabled = true;
                    startJumping = false;
                    cTime = 0;
                } else
                {
                    startJumping = false;
                    army.RemoveWarriorFromArmy(this);
                    castleToAttack.ChangeArmyValue(castleToAttack.warriorsReady + 1);
                    castleToAttack.warsJumpedinHole += 1;
                    Destroy(this.gameObject);

                }
            }
        }
    }

    void Death()
    {
        blood.Play();
        if (anim != null)
        {
            anim.SetBool("isAttacking", true);
        }
        army.RemoveWarriorFromArmy(this);
        gameObject.tag = "Untagged";
        Destroy(this.gameObject, 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.tag == "EnemyCastle" || other.gameObject.tag == "PlayerCastle" || other.gameObject.tag == "EmptyCastle") && !isDead)
        {
            if (castleToAttack != null && castleToAttack.currentArmy != null && (castleToAttack.castleBelongs != warriorBelongs))
            {
                army.AttackOtherArmy(castleToAttack.currentArmy);
                return;
            }
            if (castleToAttack != null && castleToAttack.currentArmy == null && (castleToAttack.castleBelongs != warriorBelongs) && !goingInsideCastle && army.startMoving)
            {
                 goingInsideCastle = true;
                 army.JumpToCastle(castleToAttack);
                 return;
            }
            if (castleToAttack != null && castleToAttack.warriorsReady > 0 && castleToAttack.castleBelongs == warriorBelongs && !goingInsideCastle)
            {
                castleToAttack.ChangeArmyValue(castleToAttack.warriorsReady + 1);
                army.RemoveWarriorFromArmy(this);
                Destroy(this.gameObject);
            }
        }
    }


}
