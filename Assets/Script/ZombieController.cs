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
        private bool dying = false;
        private bool attacking = false;
        private bool isDead = false;
        private int health = 10;
        private Animator animator;
        private Rigidbody rigidBody;
        private NavMeshAgent navAgent;
        private string curr_clip;

        [SerializeField]
        private float attackRadius = 0.5f; // can remove

        private GameObject attackPoint;

        [SerializeField]
        private LayerMask enemyLayer;

        protected IGameModeService gameModeService;
        protected CharacterBehaviour playerCharacter;

        private GameObject playerAgent;

        private int shootingDistance = 15;
        private float distanceWithPlayerThreshold = 10.0f;

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
            
            attackPoint = transform.Find("AttackPoint").gameObject; // can remove
            animator = GetComponent<Animator>();
            rigidBody = GetComponent<Rigidbody>();
            //rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            navAgent = GetComponent<NavMeshAgent>();
            animator.SetTrigger("setWalk");

            //Get Game Mode Service. Very useful to get Game Mode references.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            //Get Player Character.
            playerCharacter = gameModeService.GetPlayerCharacter();

            playerAgent = GameObject.Find("PlayerAgent");

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
            

            

            // // Zombie patrolling
            // waypoints = getWayPoints();
            // // Randomly pick 1 of the points to set destination
            // int index = Random.Range(0,3);
            // navAgent.SetDestination(waypoints[index]);


            // For check stucking
            lastCheckPos = transform.position;
            lastCheckTime = Time.time;

        }

        // Update is called once per frame
        void Update()
        {
            processHit(); //process zombie getting hit

            checkStuck();
            checkBoundaries();

            if (!isDead){
                // Current clip name
                curr_clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                
                // Patrolling
                //patrol();

                //navAgent.SetDestination(playerCharacter.transform.position);
                //navAgent.SetDestination(playerAgent.transform.position);

                //if (Vector3.Distance(transform.position, playerCharacter.transform.position) < 1.0f)
                if (Vector3.Distance(this.transform.position, playerAgent.transform.position) < 1.0f)
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

                

                // if (dying == false && attacking == false && curr_clip != "Zombie Attack")
                // {
                //     //transform.Translate(0, 0, 0.005f);
                //     //Vector3 movement = new Vector3(0.0f, 0.0f, 0.5f);
                //     //rigidBody.MovePosition(transform.position + movement * Time.deltaTime * 1);
                //     navAgent.isStopped = false;
                // }
                // else
                // {
                //     navAgent.isStopped = true;
                // }
            }
            
        }

        private void patrol()
        {
            if (calculateWalkingDistanceWithPlayer() > distanceWithPlayerThreshold)
            {
                // Check if current navmesh destination reached
                if (navAgent.remainingDistance < 1.0f)
                {
                    // Randomly pick 1 of the points to set destination
                    int index = Random.Range(0,3);
                    navAgent.SetDestination(waypoints[index]);
                }

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


        public override void OnEpisodeBegin()
        {
            // For check stucking
            lastCheckPos = transform.position;
            lastCheckTime = Time.time;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Target and Agent position
            sensor.AddObservation(playerCharacter.transform.localPosition); // size = 3
            sensor.AddObservation(this.transform.localPosition); // size = 3

            // Agent velocity
            sensor.AddObservation(rigidBody.velocity.x);
            sensor.AddObservation(rigidBody.velocity.z);

        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5, Color.black);
            if (!isDead)
            {
                if (dying == false && attacking == false && curr_clip != "Zombie Attack")
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5, Color.blue);
                    rigidBody.velocity = new Vector3(Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f) * 8, 0f, Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f) * 8);
                    this.transform.Rotate(0.0f, Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) * 5, 0.0f);
                    
                    AddReward(actionBuffers.ContinuousActions[2] * -0.01f); // Penalty for rotating too much

                    //AddReward(1f/Vector3.Distance(transform.position, playerAgent.transform.position)); // more near more reward
                    float walkingDistanceWithPlayer = calculateWalkingDistanceWithPlayer();
                    if (walkingDistanceWithPlayer != 0) // avoid divide 0
                        AddReward(1f/calculateWalkingDistanceWithPlayer()); // more near more reward
                    AddReward(-2f/MaxStep); // penalize each step
                }
                else
                {
                    rigidBody.velocity = Vector3.zero;
                }


                // if (calculateWalkingDistanceWithPlayer() < distanceWithPlayerThreshold)
                // {
                //     // let ml agent take control when zombie is near player
                //     navAgent.isStopped = true; // stop the navmeshagent

                //     if (dying == false && attacking == false && curr_clip != "Zombie Attack")
                //     {
                //         Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5, Color.blue);
                //         rigidBody.velocity = new Vector3(Mathf.Clamp(actionBuffers.ContinuousActions[0], 0f, 4f), 0f, Mathf.Clamp(actionBuffers.ContinuousActions[1], 0f, 4f));
                //         this.transform.Rotate(0.0f, Mathf.Clamp(actionBuffers.ContinuousActions[2], 0f, 5f), 0.0f);
                //         AddReward(Vector3.Distance(transform.position, playerAgent.transform.position)/100f);
                //     }
                //     else
                //     {
                //         rigidBody.velocity = Vector3.zero;
                //     }

                // }
                // else
                // {
                //     if (dying == false && attacking == false && curr_clip != "Zombie Attack")
                //     {
                //         navAgent.isStopped = false;
                //     }
                //     else
                //     {
                //         navAgent.isStopped = true; // if attacking stop navmesh agent
                //     }

                // }
            }
            
        }

        // public void hitPenalty(float distance)
        // {
        //     AddReward(-5f/distance);
        // }

        private float calculateWalkingDistanceWithPlayer()
        {
            NavMeshPath path = new NavMeshPath();
            float output = 0.0f;
            NavMesh.CalculatePath(transform.position, playerAgent.transform.position, NavMesh.AllAreas, path);
            for (int i = 0; i < path.corners.Length - 1; i++)
                output += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            return output;
        }

        // yq - Can remove
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
                health -= 2;
                isHit = false;
                AddReward(-0.1f);
            }
            else if(dying == false && health <= 0)
            {
                //Start the dying coroutine
                StartCoroutine(Dying());
                dying = true;
            }
            else if (isDead == true)
            {
                EndEpisode();
                Destroy(gameObject);
            }
        }

        private IEnumerator Dying()
        {
            animator.SetTrigger("setDead");
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
            //if (Vector3.Distance(transform.position, playerCharacter.transform.position) < 2.0f)
            if (Vector3.Distance(transform.position, playerAgent.transform.position) < 3.0f)
            {
                //playerCharacter.TakeDamage(10);
                AddReward(5.0f);
                Debug.Log("ZombieAttackSucess");
                EndEpisode();
            }
        }

        private void checkStuck()
        {
            if ((Time.time - lastCheckTime) > stuckSeconds)
            {
                //Debug.Log(Time.time);
                lastCheckTime = Time.time;
                if ((transform.position - lastCheckPos).magnitude < 0.25f)
                {
                    lastCheckPos = transform.position;
                    AddReward(-0.5f);
                    EndEpisode();
                    Destroy(gameObject);
                }  
                lastCheckPos = transform.position;
                
            }
        }

        private void checkBoundaries()
        {
            if (this.transform.position.x < -39 || this.transform.position.x > 4)
            {
                AddReward(-0.05f);
                EndEpisode();
                Destroy(gameObject);
            }

            if (this.transform.position.z < -39 || this.transform.position.z > 4)
            {
                AddReward(-0.05f);
                EndEpisode();
                Destroy(gameObject);
            }
        
        }
    }
}