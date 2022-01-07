using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace InfimaGames.LowPolyShooterPack
{

    public class PlayerAgent : Agent
    {
        private Rigidbody rigidBody;
        private LayerMask mask;

        private int walkSpeed = 4;
        private int rotationSpeed = 3;

        private int shootingDistance = 20;

        GameObject[] zombieObjects;

        private Vector3 lastCheckPos;
        private float lastCheckTime;
        private float stuckSeconds = 15.0f;

        private float shootingInterval = 0.1f;
        private float lastHitTime = 0f;

        // Start is called before the first frame update
        void Start()
        {
            mask = LayerMask.GetMask("Zombie");
            rigidBody = GetComponent<Rigidbody>();
            lastCheckTime = Time.time;
            lastCheckPos = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            checkBoundaries();
            checkStuck();
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 20, Color.blue);
            // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 15, mask))
            // {
            //     Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 15, Color.blue);
            // }
        }

        public override void OnEpisodeBegin()
        {
            //this.transform.position = new Vector3(-20f, 2.0f, -50f);
            //this.transform.position = new Vector3(-26.2f, 2.0f, -26.4f);
            this.transform.position = new Vector3(Random.Range(-37.0f, -12.0f), 2.0f, Random.Range(-36.0f, -14.0f));
            
            rigidBody.velocity = Vector3.zero;

            // For check stucking
            lastCheckPos = transform.position;
            lastCheckTime = Time.time;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            zombieObjects = GameObject.FindGameObjectsWithTag("Zombie");
            if (zombieObjects.Length > 0)
            {
                sensor.AddObservation(zombieObjects[0].transform.localPosition);
                //AddReward(Vector3.Distance(zombieObjects[0].transform.localPosition, transform.localPosition)/-1000f);
            }
            else
            {
                sensor.AddObservation(Vector3.zero);
            }
            
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Vector3 controlSignal = Vector3.zero;
            // controlSignal.x = actionBuffers.ContinuousActions[0];
            // controlSignal.z = actionBuffers.ContinuousActions[1];
            // rigidBody.AddForce(controlSignal * 10);

            // move agent
            rigidBody.velocity = new Vector3(Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f) * walkSpeed, 0f, Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f) * walkSpeed);
            //Vector3 move = new Vector3(actionBuffers.ContinuousActions[0] * walkSpeed, 0f, actionBuffers.ContinuousActions[1] * walkSpeed) * Time.deltaTime;
            //rigidBody.MovePosition(move);

            // rotate agent
            this.transform.Rotate(0.0f, Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) * rotationSpeed, 0.0f);

            AddReward(actionBuffers.ContinuousActions[2] * -0.001f); // Penalty for rotating too much
            AddReward(-2f/MaxStep); // penalize each step
            
            //if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out var hit, shootingDistance, mask))
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out var hit, shootingDistance))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                if (hit.transform.gameObject.tag == "Zombie") // check the object it hit
                {
                    if (Time.time - lastHitTime > shootingInterval)
                    {
                        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                        Debug.Log("PlayerShoot");
                        AddReward(2.0f); // Reward agent if shot zombie
                        hit.transform.gameObject.GetComponent<ZombieController>().isHit = true;
                        //hit.transform.gameObject.GetComponent<ZombieController>().hitPenalty(hit.distance);
                        //Destroy(hit.transform.gameObject); 

                        lastHitTime = Time.time;

                    }
                    
                }
                
            }

        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Zombie")
            {
                AddReward(-0.1f); // Punish agent if zombie is too near to agent
                EndEpisode();
            }

        }

        private void checkBoundaries()
        {
            if (this.transform.position.x < -39 || this.transform.position.x > 4)
            {
                AddReward(-0.05f);
                EndEpisode();
            }

            if (this.transform.position.z < -39 || this.transform.position.z > 4)
            {
                AddReward(-0.05f);
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
                }  
                lastCheckPos = transform.position;
                
            }
        }
    }
}
