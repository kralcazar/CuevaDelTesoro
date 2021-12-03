using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int gridSize;
    public static int numAgents;
    public static int numMonsters;
    public static int numCliffs;

    public static bool playing;
    public static float simulationSpeed;

    private static GameObject[] agentsGO;
    private static Agent[] agents;

    private float timePeriod;
    [SerializeField] private float timeTick;

    [SerializeField] private GridManager gridManager;
    private static GameManager instance;

    private void Start()
    {
        if (instance == null) instance = this;
    }

    public static void InitGame()
    {
        instance.gridManager.GenerateGrid(gridSize);

        Vector2[] corners = { 
            new Vector2(0, 0),
            new Vector2(gridSize-1, 0),
            new Vector2(0, gridSize-1),
            new Vector2(gridSize-1, gridSize-1)
        };

        GameObject agentBase = Resources.Load<GameObject>("Agent");

        agentsGO = new GameObject[numAgents];
        agents = new Agent[numAgents];
        for (int i = 0; i < numAgents && i < corners.Length; i++)
        {
            agentsGO[i] = Instantiate(agentBase);
            agentsGO[i].transform.position = GridManager.GetGrid().GetWorldPosition((int)corners[i].x, (int)corners[i].y);
            agentsGO[i].transform.position += Vector3.right * 0.5f + Vector3.up * 0.5f;
            agents[i] = agentsGO[i].GetComponent<Agent>();
        }
    }

    public void StartGame()
    {
        playing = true;
    }

    void Update()
    {
        if (playing)
        {
            timePeriod += Time.deltaTime;
            if (timePeriod > timeTick)
            {
                Tick();
                timePeriod = 0;
            }
        }
    }

    private void Tick()
    {
        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].Tick();
        }
    }
}
