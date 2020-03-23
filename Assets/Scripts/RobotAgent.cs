using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAgent : Agent
{

    public float moveSpeed = 5f;
    public float turnSpeed = 180f;

    public TrainingArea trainingArea;
    public GameObject blockPrefab;

    public int type = 0; // 0=RoboBlock, 1=Helper
    private bool hasBlock = false;
    private GameObject carriedBlock = null;
    private Rigidbody rbody;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rbody = GetComponent<Rigidbody>();
        rbody.centerOfMass = Vector3.zero;
        rbody.inertiaTensorRotation = Quaternion.identity;
    }

    public override void AgentReset()
    {
      //  trainingArea.ResetArea(); // Calls cleanRobot() on all robots
    }

    public void cleanRobot()
    {
        hasBlock = false;
        carriedBlock = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void AgentAction(float[] vectorAction)
    {
        // Convert the first action to forward movement
        float forwardAmount = vectorAction[0];

        // Convert the second action to turning left or right
        float turnAmount = 0f;
        if (vectorAction[1] == 1f)
        {
            turnAmount = -1f;
        }
        else if (vectorAction[1] == 2f)
        {
            turnAmount = 1f;
        }

        // Apply movement
        rbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        if(rbody.position.y < 0)
        {
            AddReward(-10f);
            this.transform.localPosition = new Vector3(0, 0.25f, 0);
            trainingArea.FinishEpisode();
        }

        if (GetCumulativeReward() < -10f)
        {
            trainingArea.FinishEpisode();
        }

        // Apply a tiny negative reward every step to encourage action
        AddReward(-1f / 1000);
    }

    public override float[] Heuristic()
    {
        float forwardAction = 0f;
        float turnAction = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            turnAction = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            turnAction = 2f;
        }

        // Put the actions into an array and return
        return new float[] { forwardAction, turnAction };
    }

    private void FixedUpdate()
    {
        // Request a decision every 5 steps. RequestDecision() automatically calls RequestAction(),
        // but for the steps in between, we need to call it explicitly to take action using the results
        // of the previous decision
        if (GetStepCount() % 5 == 0)
        {
            RequestDecision();
        }
        else
        {
            RequestAction();
        }


        // Test if the agent is close enough to release the block
        /*     if (hasBlock && Vector3.Distance(transform.position, trainingArea.targetArea.transform.position) < 4)
             {
                 // Close enough to drop the block
                 DropBlock();
             }*/
    }

    public override void CollectObservations()
    {
        AddVectorObs(hasBlock); // 1

        AddVectorObs(transform.position); // 3

        AddVectorObs(Vector3.Distance(trainingArea.targetArea.transform.position, transform.position)); // 1

        AddVectorObs((trainingArea.targetArea.transform.position - transform.position).normalized); // 3

        // Direction facing (1 Vector3 = 3 values)
        AddVectorObs(transform.forward); // 3

        // 1 + 3 + 1 + 3 + 3 = 11 total values
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (type==0 && collision.transform.CompareTag("block"))
        {
            if(!hasBlock)
                CollectBlock(collision.gameObject);
        }
        else if (type == 0 && collision.transform.CompareTag("targetzone"))
        {
            if(hasBlock)
                DropBlock();
        }
        else if (collision.transform.CompareTag("robot"))
        {
            Debug.Log("Robot collision");
            AddReward(-0.5f);
        }
    }

    private void DropBlock()
    {
        hasBlock = false;
        carriedBlock.transform.parent = trainingArea.transform;
        carriedBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        carriedBlock.GetComponent<Rigidbody>().velocity = ((trainingArea.targetArea.transform.position - carriedBlock.transform.position) * 1.0f + new Vector3(0, 6, 0));

        //Destroy(carriedBlock);
        carriedBlock = null;

        //AddReward(0.5f);
        trainingArea.AddGroupReward(1f); // Only group reward

        trainingArea.BlockMoved();

        if (trainingArea.RemainingBlocks <= 0)
        {
            trainingArea.FinishEpisode();
        }
    }

    private void CollectBlock(GameObject block)
    {
        hasBlock = true;
        block.transform.parent = transform;
        block.transform.localPosition = new Vector3(0, transform.localScale.y+block.transform.localScale.y/2, 0);
        block.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        carriedBlock = block;

        //AddReward(0.5f);
        trainingArea.AddGroupReward(1f); // Only group reward
    }
}
