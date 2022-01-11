using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace InfimaGames.LowPolyShooterPack
{
    public class ZombieController : MonoBehaviour
    {
        public bool isHit = false;
        private bool dying = false;
        private bool attacking = false;
        private bool isDead = false;
        private int health = 10;
        private Animator animator;
        //private Rigidbody rigidBody;
        private NavMeshAgent navAgent;
        private string curr_clip;

        [SerializeField]
        private float attackRadius = 0.5f;

        private GameObject attackPoint;

        [SerializeField]
        private LayerMask enemyLayer;

        protected IGameModeService gameModeService;
        protected CharacterBehaviour playerCharacter;


        // Start is called before the first frame update
        void Start()
        {
            attackPoint = transform.Find("AttackPoint").gameObject;
            animator = GetComponent<Animator>();
            //rigidBody = GetComponent<Rigidbody>();
            //rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            navAgent = GetComponent<NavMeshAgent>();
            animator.SetTrigger("setWalk");

            //Get Game Mode Service. Very useful to get Game Mode references.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            //Get Player Character.
            playerCharacter = gameModeService.GetPlayerCharacter();
        }

        // Update is called once per frame
        void Update()
        {
            processHit(); //process zombie getting hit

            if (!isDead){
                // Current clip name
                curr_clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                
                navAgent.SetDestination(playerCharacter.transform.position);

                if (Vector3.Distance(transform.position, playerCharacter.transform.position) < 1.0f)
                {
                    if (attacking == false)
                    {
                        // animator.SetTrigger("setAttack");
                        attacking = true;
                        // playerCharacter.TakeDamage(10);
                        StartCoroutine(ZombieAttacking());
                    }

                }
                else
                {
                    attacking = false;
                    animator.SetTrigger("setWalk");
                }

                

                if (dying == false && attacking == false && curr_clip != "Zombie Attack")
                {
                    //transform.Translate(0, 0, 0.005f);
                    //Vector3 movement = new Vector3(0.0f, 0.0f, 0.5f);
                    //rigidBody.MovePosition(transform.position + movement * Time.deltaTime * 1);
                    navAgent.isStopped = false;
                }
                else
                {
                    navAgent.isStopped = true;
                }
            }
            
        }

        // Attack palyer if near to zombie
        private void attackPlayer()
        {
            Collider[] hitColliders = Physics.OverlapSphere(attackPoint.transform.position, attackRadius, enemyLayer);
            foreach (var hitCollider in hitColliders)
            {
                // Only process if collided with player
                if (hitCollider.name == "Player")
                {
                    //Debug.Log("Hit " + hitCollider.name);
                    if (attacking == false)
                    {
                        animator.SetTrigger("setAttack");
                        attacking = true;
                    }
                }
                
            }

        }

        // Process if bullet has hitted zombie
        private void processHit()
        {
            if (isHit == true)
            {
                health -= 5;
                isHit = false;
            }
            else if(dying == false && health <= 0)
            {
                //Start the dying coroutine
                StartCoroutine(Dying());
                dying = true;
            }
            else if (isDead == true)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator Dying()
        {
            animator.SetTrigger("setDead");
            GameManager.enemyKilled += 1;
            yield return new WaitForSeconds(5);
            isDead = true;
        }

        private IEnumerator ZombieAttacking()
        {
            animator.SetTrigger("setAttack");
            navAgent.isStopped = true;
            yield return new WaitForSeconds(2.5f);
            if (Vector3.Distance(transform.position, playerCharacter.transform.position) < 2.0f)
            {
                playerCharacter.TakeDamage(10);
            }
        }
    }
}