using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool debug;

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
    [SerializeField] private InputManager inputManager;
    private static GameManager instance;

    private void Start()
    {
        if (instance == null) instance = this;
        if (debug)
        {
            gridSize = 8;
            numAgents = 1;
            InitGame();
        }
    }

    public void ResetInstance()
    {
        //Hay celdas que quedan huérfanas al eliminar los agentes,
        //Se han borrado antes de ser añadidas a la lista de borrado.
        //las borramos ahora...
        DebugCell[] cells = FindObjectsOfType<DebugCell>();
        foreach (DebugCell cell in cells)
        {
            Destroy(cell.gameObject); 
        }
        //Lo mismo con los agentes que no han empezado un juego
        Agent[] agents = FindObjectsOfType<Agent>();
        foreach (Agent a in agents)
        {
            Destroy(a.gameObject);
        }

        //Reiniciamos la instancia
        instance = this;
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
            agentsGO[i].transform.position += 
                Vector3.right * GridManager.GetGrid().GetCellSize()/2 
                + Vector3.up * GridManager.GetGrid().GetCellSize() / 2;
            agents[i] = agentsGO[i].GetComponent<Agent>();
        }
    }

    public void StartGame()
    {
        timePeriod = timeTick; //Empezar el primer paso de la simulación al instante
        playing = true;
    }

    public void StopGame()
    {
        playing = false;
    }

    public static void EndGame()
    {
        playing = false;

        foreach (Agent a in agents)
        {
            a.gameObject.SetActive(false);
            foreach (GameObject cell in a.debugCells)
            {
                DestroyImmediate(cell);
            }
            Destroy(a.gameObject);
        }

        instance.inputManager.GameOver();
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
