using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class TrainingArea : MonoBehaviour
{

    public GameObject blockPrefab;
    public GameObject robotPrefab;
    public GameObject targetArea;
    public TextMesh TextScore;

    private List<RobotAgent> robots = new List<RobotAgent>();
    private List<GameObject> blocks = new List<GameObject>();
    private int remainingBlocks = 10;
    private int targetRemainingBlocks = 10;
    private int numRobots = 0;

    // Start is called before the first frame update
    void Start() {
        ResetArea();
    }

    // Update is called once per frame
    void Update()
    {
        PublishScore();        
    }

    public void FinishEpisode()
    {
        foreach (RobotAgent r in robots)
        {
            r.Done();
        }
        ResetArea();
    }

    public void AddGroupReward(float reward)
    {
        foreach (RobotAgent r in robots)
        {
            r.AddReward(reward);
        }
    }

    public void PublishScore()
    {
        float score = 0;
        foreach (RobotAgent r in robots)
        {
            score += r.GetCumulativeReward();
        }
        score /= robots.Count;
        TextScore.text = score.ToString("F2") + " (" + remainingBlocks + ")";
    }

    public void ResetArea()
    {
        RegenerateRobots();
        GenerateBlocks(10);
        int newTargetRemainingBlocks = (int)Academy.Instance.FloatProperties.GetPropertyWithDefault("number_blocks", 10.0f);
        if (newTargetRemainingBlocks != targetRemainingBlocks)
        {
            targetRemainingBlocks = newTargetRemainingBlocks;
            Debug.Log("Now blocks are: " + targetRemainingBlocks);
        }
        remainingBlocks = targetRemainingBlocks;
    }

    public void RegenerateRobots()
    {
        // Clean properties of existing robots
        foreach (RobotAgent r in robots)
        {
            r.cleanRobot();
        }

        int newNumberRobots = (int)Academy.Instance.FloatProperties.GetPropertyWithDefault("number_robots", 2.0f);
        if (newNumberRobots < numRobots)
        { // Destroy exceeding robots
            for (int i = newNumberRobots; i < numRobots; i++)
            {
                Destroy(robots[i]);
            }
            numRobots = newNumberRobots;
            Debug.Log("Now robots are: " + numRobots);
        }
        else if (newNumberRobots > numRobots)
        {
            for (int i = numRobots; i < newNumberRobots; i++)
            {
                GameObject robot = Instantiate(robotPrefab);
                robot.transform.SetParent(transform);
                robot.transform.localPosition = new Vector3(UnityEngine.Random.Range(-2, 4), 0.15f, UnityEngine.Random.Range(-2, 4));
                RobotAgent agent = robot.GetComponent<RobotAgent>();
                agent.trainingArea = this;
                if (i == 1)
                {
                    agent.GiveModel("RoboHelp", null);
                    agent.type = 1;
                }
                else
                {
                    agent.GiveModel("RoboBlock", null);
                    agent.type = 0;
                    agent.moveSpeed = 3f; // Slower 
                }
                robots.Add(agent);
            }
            numRobots = newNumberRobots;
            Debug.Log("Now robots are: " + numRobots);
        }
    }

    public void GenerateBlocks(int quantity)
    {
        // First destroy existing blocks if any

        if(blocks != null)
        {
            foreach(GameObject b in blocks)
            {
                Destroy(b);
            }
        }
        blocks = new List<GameObject>();
        for(int i=0; i < quantity; i++)
        {
            GameObject block = Instantiate(blockPrefab);
            block.transform.SetParent(transform);
            block.transform.localPosition = new Vector3(UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0.5f, 1), UnityEngine.Random.Range(0, 4));
            blocks.Add(block);
        }

    }

    public void BlockMoved()
    {
        remainingBlocks--;
    }

    public int RemainingBlocks
    {
        get{ return remainingBlocks; }
    }

}
