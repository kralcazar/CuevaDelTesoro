using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int gridSize;
    public static int numPlayers;
    public static int numMonsters;
    public static int numCliffs;

    public static float simulationSpeed;

    [SerializeField] private Agent[] agents;

    private float timePeriod;
    [SerializeField] private float timeTick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timePeriod += Time.deltaTime;
        if (timePeriod > timeTick)
        {
            Tick();
            timePeriod = 0;
        }
    }

    private void Tick()
    {

    }
}
