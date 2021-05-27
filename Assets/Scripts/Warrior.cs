using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Warrior : MonoBehaviour
{
    //Stats
    public bool isEnemy;
    //
    bool inDuel;
    Warrior warToAttack;
    [HideInInspector] public Army army;
    Army armyToAttack;
    [HideInInspector] public NavMeshAgent agent;
    Animator anim;
    Renderer render;
    [HideInInspector] public bool attacking;
    bool goingInsideCastle;
    bool isDead;
    bool jumpToCastle;
    CastleBehaviour castleToAttack;
    public CastleBehaviour.Belongs warriorBelongs;

    Color blueColor = new Color(0.1830188f, 0.5300552f, 1f, 1f);


    //JumpStats
    Vector3 startPos;
    Vector3 endPos;
    public float trajectoryHeight = 5;
    public float jumpSpeed;
    bool startJumping;
    float cTime;
    //
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        render = transform.GetChild(1).GetComponent<Renderer>();
        CheckBelongs();
        StartJump(true, Vector3.zero);
    }

    public void StartJump(bool fromCastle, Vector3 positionToJump)
    {
            agent.enabled = false;
            startPos = transform.position;
            if (fromCastle)
            {
                Vector3 randomPoint = transform.position + Random.onUnitSphere * 0.5f;
                endPos = new Vector3(randomPoint.x, 0.01f, randomPoint.z);
            }
            else
            {
                endPos = positionToJump;
                jumpToCastle = true;
            }
            Vector3 posToLook = new Vector3(endPos.x, transform.position.y, endPos.z);
            transform.LookAt(posToLook);
            startJumping = true;
    }

    public void CheckBelongs()
    {
        if (isEnemy)
        {
            warriorBelongs = CastleBehaviour.Belongs.Enemy;
            render.material.color = Color.red;
            gameObject.tag = "EnemyWarrior";
        }
        else
        {
            warriorBelongs = CastleBehaviour.Belongs.Player;
            render.material.color = blueColor;

            gameObject.tag = "PlayerWarrior";
        }
    }

    public void StopWarrior(Vector3 positionOfStop)
    {
        agent.enabled = true;
        attacking = true;
        //agent.enabled = false;
        agent.destination = positionOfStop;
    }

    public void AttackWarrior(Warrior war, Vector3 position)
    {
        agent.enabled = true;
        warToAttack = war;
        attacking = true;
        inDuel = true;
        agent.destination = position;
    }

    public void MoveToPosition(Vector3 destanation)
    {
        agent.destination = destanation;
    }

    void FollowArmy()
    {
        if (agent.enabled == true && !goingInsideCastle && !attacking && !inDuel)
            agent.destination = army.gameObject.transform.position;
    }

    public void AttackCastle(CastleBehaviour castle)
    {
        castleToAttack = castle;
        agent.destination = castleToAttack.transform.position;
        attacking = true;
        // StartCoroutine("FightChecker", castle);
    }

    private void Update()
    {
        FollowArmy();
        if (agent.velocity.magnitude > 0.1f)
        {
            anim.SetBool("isRunning", true);
        } else anim.SetBool("isRunning", false);
        if (agent.velocity.magnitude < 1f && attacking)
        {
            // anim.SetBool("isAttacking", true);
        }

        if (goingInsideCastle && Vector3.Distance(agent.destination, transform.position) < 0.01f)
        {
            //army.RemoveWarriorFromArmy(this);
            //Destroy(this.gameObject);
        }

        if (inDuel)
        {
            if (Vector3.Distance(transform.position, agent.destination) < 0.3f)
            {

                warToAttack.Death();
                Death();
            }
        }

        if (startJumping)
        {
            cTime += jumpSpeed * Time.deltaTime;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, cTime);
            currentPos.y += trajectoryHeight * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);
            transform.position = currentPos;
            if (Vector3.Distance(transform.position, endPos) < 0.01f)
            {
                if (!jumpToCastle)
                {
                    agent.enabled = true;
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
        if (agent != null)
        agent.enabled = false;
        if (anim != null)
        {
            anim.SetBool("isAttacking", true);
            anim.SetBool("isDead", true);
        }
        if (army != null && this != null)
            army.RemoveWarriorFromArmy(this);
        gameObject.tag = "Untagged";
        Destroy(this.gameObject, 1.5f);
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
            if (castleToAttack != null && castleToAttack.currentArmy == null && (castleToAttack.castleBelongs != warriorBelongs) && !goingInsideCastle)
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
        if (other.gameObject.tag == "Spike") Death();
    }

}
