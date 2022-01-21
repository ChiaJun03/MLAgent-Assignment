using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


namespace InfimaGames.LowPolyShooterPack
{
    public class ZombieController : Agent
    {
        public bool isHit = false;
        public AudioSource zombieAudio;
        public AudioClip zombieShout;
        private bool dying = false;
        private bool attacking = false;
        private bool isDead = false;
        private int health = 10;
        private Animator animator;
        private Rigidbody rigidBody;
        private NavMeshAgent navAgent;
        private string curr_clip;

        // [SerializeField]
        // private float attackRadius = 0.5f;

        // private GameObject attackPoint;

        // [SerializeField]
        // private LayerMask enemyLayer;

        protected IGameModeService gameModeService;
        protected CharacterBehaviour playerCharacter;

        //private int shootingDistance = 15;
        private float distanceWithPlayerThreshold = 25.0f;

        [Header("Patrolling Waypoints")]
        private Vector3[] staffLoungeWaypoints;
        private Vector3[] waitingAreaWaypoints;
        private Vector3[] openAreaWaypoints;

        private int areaIndex;

        private Vector3[] waypoints;

        private Vector3 lastCheckPos;
        private float lastCheckTime;
        private float stuckSeconds = 15.0f;


        // Start is called before the first frame update
        void Start()
        {
            //attackPoint = transform.Find("AttackPoint").gameObject;
            animator = GetComponent<Animator>();
            rigidBody = GetComponent<Rigidbody>();
            //rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            navAgent = GetComponent<NavMeshAgent>();
            zombieAudio = GetComponent<AudioSource>();
            animator.SetTrigger("setWalk");

            //Get Game Mode Service. Very useful to get Game Mode references.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            //Get Player Character.
            playerCharacter = gameModeService.GetPlayerCharacter();

            //playerAgent = GameObject.Find("Player");

            // Initialize waypoints coordinates            
            staffLoungeWaypoints = new Vector3[] {new Vector3(-35.9162064f,1.58333337f,-1.01413155f), 
                                                    new Vector3(-29.9578114f,1.58333337f,-6.6531086f), 
                                                    new Vector3(-21.4458199f,1.58333337f,-17.9310646f)};
            waitingAreaWaypoints = new Vector3[] {new Vector3(2.59578705f,1.58333337f,-30.2433643f),
                                                     new Vector3(-11.0233994f,1.58333337f,-21.7848969f), 
                                                     new Vector3(-5.91620636f,1.58333337f,-37.2920876f)};
            openAreaWaypoints = new Vector3[] {new Vector3(0.339790344f,1.58333337f,1.16637993f), 
                                                new Vector3(-18.6602097f,1.58333337f,-6.83362007f), 
                                                new Vector3(-4.66020966f,1.58333337f,-16.8336201f)};
            
            // Zombie patrolling
            waypoints = getWayPoints();
            // Randomly pick 1 of the points to set destination
            int index = Random.Range(0,3);
            navAgent.SetDestination(waypoints[index]);


            // For check stucking
            lastCheckPos = transform.position;
            lastCheckTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            processHit(); //process zombie getting hit

            // if (!isDead){
            //     // Current clip name
            //     curr_clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                
            //     navAgent.SetDestination(playerCharacter.transform.position);

            //     if (Vector3.Distance(transform.position, playerCharacter.transform.position) < 1.0f)
            //     {
            //         if (attacking == false)
            //         {
            //             // animator.SetTrigger("setAttack");
            //             attacking = true;
            //             // playerCharacter.TakeDamage(10);
            //             StartCoroutine(ZombieAttacking());
            //         }

            //     }
            //     else
            //     {
            //         attacking = false;
            //         animator.SetTrigger("setWalk");
            //     }

                

            //     if (dying == false && attacking == false && curr_clip != "Zombie Attack")
            //     {
            //         //transform.Translate(0, 0, 0.005f);
            //         //Vector3 movement = new Vector3(0.0f, 0.0f, 0.5f);
            //         //rigidBody.MovePosition(transform.position + movement * Time.deltaTime * 1);
            //         navAgent.isStopped = false;
            //     }
            //     else
            //     {
            //         navAgent.isStopped = true;
            //     }
            // }
            
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
                zombieAudio.PlayOneShot(zombieShout, 1.0f);
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
            AddReward(1.0f);
            Debug.Log("ZombieAttack");
            if (Vector3.Distance(transform.position, playerCharacter.transform.position) < 2.0f)
            {
                playerCharacter.TakeDamage(10);
                AddReward(5.0f);
                Debug.Log("ZombieAttackSucess");
            }
            attacking = false;
            animator.SetTrigger("setWalk");
        }

        private void patrol()
        {
            // Check if current navmesh destination reached
            if (navAgent.remainingDistance < 1.0f)
            {
                // Randomly pick 1 of the points to set destination
                int index = Random.Range(0,3);
                navAgent.SetDestination(waypoints[index]);
            }
        }

        private Vector3[] getWayPoints()
        {
            if (this.transform.position.x > -39.1 && this.transform.position.x < -27.5 && this.transform.position.z > -8.9 && this.transform.position.z < 3)
            {
                return staffLoungeWaypoints; // Staff Lounge Area
            }
            if (this.transform.position.x > -8 && this.transform.position.x < 4 && this.transform.position.z > -38 && this.transform.position.z < -21.5)
            {
                return waitingAreaWaypoints; // Waiting Area
            }
            if (this.transform.position.x > -21 && this.transform.position.x < 3 && this.transform.position.z > -17 && this.transform.position.z < 3)
            {             
                return openAreaWaypoints; // Open Area
            }
            return openAreaWaypoints;
        }

        private float calculateWalkingDistanceWithPlayer()
        {
            NavMeshPath path = new NavMeshPath();
            float output = 0.0f;
            NavMesh.CalculatePath(transform.position, playerCharacter.transform.position, NavMesh.AllAreas, path);
            for (int i = 0; i < path.corners.Length - 1; i++){
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.yellow);
                output += Vector3.Distance(path.corners[i], path.corners[i + 1]);}
            //Debug.Log("distance: "+output);
            return output;
        }

        public override void OnEpisodeBegin()
        {
            // For check stucking
            lastCheckPos = transform.position;
            lastCheckTime = Time.time;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Target and Agent position
            //sensor.AddObservation(playerCharacter.transform.localPosition); // size = 3
            //sensor.AddObservation(this.transform.localPosition); // size = 3

            // Agent velocity
            //sensor.AddObservation(rigidBody.velocity.x);
            //sensor.AddObservation(rigidBody.velocity.z);

            // Get direction to player in x and z axis
            Vector3 dirToPlayer = (playerCharacter.transform.localPosition - transform.localPosition).normalized;
            sensor.AddObservation(dirToPlayer.x);
            sensor.AddObservation(dirToPlayer.z);

            // get whether zombie is in line of site of player
            Vector3 dirToZombie = (transform.localPosition - playerCharacter.transform.localPosition).normalized;
            float dotProd = Vector3.Dot(dirToZombie, playerCharacter.transform.forward);
            sensor.AddObservation(dotProd);

        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (!isDead) 
            {

                if (calculateWalkingDistanceWithPlayer() < distanceWithPlayerThreshold)
                {
                    // let ml agent take control when zombie is near player
                    navAgent.isStopped = true; // stop the navmeshagent

                    if (dying == false && attacking == false && curr_clip != "Zombie Attack")
                    {
                        // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5, Color.blue);
                        // rigidBody.velocity = new Vector3(Mathf.Clamp(actionBuffers.ContinuousActions[0], 0f, 4f), 0f, Mathf.Clamp(actionBuffers.ContinuousActions[1], 0f, 4f));
                        // this.transform.Rotate(0.0f, Mathf.Clamp(actionBuffers.ContinuousActions[2], 0f, 5f), 0.0f);
                        // AddReward(Vector3.Distance(transform.position, playerAgent.transform.position)/100f);

                        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
                        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
                        float moveSpeed = 50f;

                        Vector3 addForce = new Vector3(moveX, 0, moveZ) * moveSpeed;
                        rigidBody.velocity = addForce + new Vector3(0, rigidBody.velocity.y, 0);
                        // var temp = addForce + new Vector3(0, rigidBody.velocity.y, 0);
                        // Debug.Log(temp);
                        // rigidBody.AddForce(temp, ForceMode.VelocityChange);

                        float rotateY = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
                        float rotateSpeed = 5f;

                        transform.Rotate(0.0f, rotateY * rotateSpeed, 0.0f);

                        int attack = actions.DiscreteActions[0];
                        Debug.DrawRay(transform.position, transform.forward * 3.0f, Color.blue);
                        if (attack == 1) 
                        {
                            attacking = true;
                            //rigidBody.velocity = Vector3.zero;
                            StartCoroutine(ZombieAttacking());
                        }
                        Debug.Log(moveX + " " + moveZ + " " + rotateY + " " + attack);
                        Debug.Log(rigidBody.velocity);
                    }
                    // else
                    // {
                    //     rigidBody.velocity = Vector3.zero;
                    // }

                }
                else
                {
                    if (dying == false && attacking == false && curr_clip != "Zombie Attack")
                    {
                        navAgent.isStopped = false;
                    }
                    else
                    {
                        navAgent.isStopped = true; // if attacking or dying stop navmesh agent
                    }
                    patrol();

                }

                // float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
                // float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
                // float moveSpeed = 10f;

                // Vector3 addForce = new Vector3(moveX, 0, moveZ) * moveSpeed;
                // rigidBody.velocity = addForce + new Vector3(0, rigidBody.velocity.y, 0);

                // float rotateY = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
                // float rotateSpeed = 5f;

                // transform.Rotate(0.0f, rotateY * rotateSpeed, 0.0f);

                // int attack = actions.DiscreteActions[0];
                // Debug.DrawRay(transform.position, transform.forward * 3.0f, Color.blue);
                // if (attack == 1) {
                //     StartCoroutine(ZombieAttacking());
                // }
                // Debug.Log(moveX + " " + moveZ + " " + rotateY + " " + attack);
            }
        }
    }
}