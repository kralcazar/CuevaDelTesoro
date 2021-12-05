using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Directions { West, North, East, South }
public class Action{ public Directions direction;}

public class Agent : MonoBehaviour
{
    [SerializeField]
    private KnowledgeBase kb;
    [SerializeField]
    private GameObject debugCellPrefab;

    int[] lookX = { -1, 0, 1, 0 };
    int[] lookY = { 0, 1, 0, -1 };

    private void Start()
    {
        kb = new KnowledgeBase(this); 
        
        int x, y;
        GridManager.GetGrid().GetXY(transform.position, out x, out y);
        kb.InformAction(new Vector2(x, y));
        PercieveAndInformEnvironmentCell(x, y);
        PercieveAndInformEnvironmentSurroundings(x, y); // Paso 0: Percibir la primera celda e informar a la base de conocimientos
        TryToInferCell(x, y);
        TryToInferSurroundings(x, y); // Paso 0: Inferir conocimiento en la base de conocimientos
    }

    //Se llama una vez por cada iteracción del juego
    public void Tick()
    {
        int x, y;
        GridManager.GetGrid().GetXY(transform.position, out x, out y);

        if (AskForActionsDeductive(x, y) != null) // Paso 3: Toma de decisiones
        {
            GridManager.GetGrid().GetXY(transform.position, out x, out y); //Tenemos que obtener la nueva posición del agente
            kb.InformAction(new Vector2(x, y)); // Paso 4: Informar de la acción tomada (útil para marcar la celda ya visitada)
            
            //Si ha encontrado un tesoro lo elimina de la casilla
            CellType cellType = GridManager.GetGrid().GetGridObject(x, y).GetCellType();
            if (cellType == CellType.Tresor)
            {
                kb.InformTresor(new Vector2(x, y));
                GridManager.GetGrid().GetGridObject(transform.position).SetCellType(CellType.Empty);
            }

            PercieveAndInformEnvironmentSurroundings(x, y); // Paso 1: Percibir ambiente e informar a la base de conocimientos

            TryToInferSurroundings(x, y); // Paso 2: Inferir conocimiento en la base de conocimientos

        }
        else
        {
            Debug.LogWarning("No actions allowed");
            //Backtracking
            //TODO
        }
    }

    //Recibir percepciones del entorno e informar a la base de conocimientos
    private void PercieveAndInformEnvironmentSurroundings(int x, int y)
    {
        for (int i = 0; i < lookX.Length; i++) // Mirar alrededor
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            PercieveAndInformEnvironmentCell(_x, _y);
        }
    }
    //Recibir percepciones en una casilla e informar a la base de conocimientos
    private void PercieveAndInformEnvironmentCell(int x, int y)
    {
        bool[] percpt = Enviroment.GetPerception(new Vector2(x, y));
        kb.Inform(new Vector2(x, y), percpt);
    }

    private void TryToInferSurroundings(int x, int y)
    {
        for (int i = 0; i < lookX.Length; i++) // Inferir alrededor
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            TryToInferCell(_x, _y);
        }
        for (int i = 0; i < lookX.Length; i++) // Inferir conocimiento
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            TryToInferCell(_x, _y);
        }
    }
    private void TryToInferCell(int x, int y)
    {
        kb.InferPerceptionRules(new Vector2(x, y));
        kb.InferKnowledgeRules(new Vector2(x, y));
    }

    //Devuelve la primera acción aplicable (agente reactivo)
    private Action AskForActionsReactive(int x, int y)
    {
        Directions[] directions = { Directions.West, Directions.North, Directions.East, Directions.South };
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];

            if (kb.Ask(new Vector2(_x, _y)))
            {
                Action action = new Action();
                action.direction = directions[i];
                Vector3 targetPosition = GridManager.GetGrid().GetWorldPosition(_x, _y);
                Debug.Log("MoveTo: " + targetPosition);
                transform.position = targetPosition;
                return action; 
            }
        }
        return null;
    }

    //Devuelve la acción con mayor prioridad (agente deductivo)
    private Action AskForActionsDeductive(int x, int y)
    {
        Directions[] directions = { Directions.West , Directions.North, Directions.East, Directions.South};
        Action[] actions = new Action[lookX.Length];
        int bestPriority = int.MinValue;
        int bestActionIndex = -1;

        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            int priority = kb.AskPriority(new Vector2(_x, _y));

            Debug.Log("priority: "+ priority);

            if (priority > bestPriority) //profundidad
            {
                bestPriority = priority;
                bestActionIndex = i;
            }

            actions[i] = new Action();
            actions[i].direction = directions[i];
        }

        if (bestPriority == int.MinValue) return null;

        Debug.Log("bestActionIndex: " + bestActionIndex);
        Debug.Log("bestActionIndex: " + 
            new Vector2( x + lookX[bestActionIndex], y + lookY[bestActionIndex]));

        Vector3 targetPosition = GridManager.GetGrid().GetWorldPosition(x + lookX[bestActionIndex], y + lookY[bestActionIndex]);
        transform.position = targetPosition;
        transform.position +=
                Vector3.right * GridManager.GetGrid().GetCellSize() / 2
                + Vector3.up * GridManager.GetGrid().GetCellSize() / 2;

        Debug.Log("Target cell type: "+GridManager.GetGrid().GetGridObject(x + lookX[bestActionIndex], y + lookY[bestActionIndex]).GetCellType());

        return actions[bestActionIndex];
    }

    public void ShowKnowledge(Vector2 gridPosition, string text)
    {
        GameObject debugCell = Instantiate(debugCellPrefab);
        Text textGrid = debugCell.GetComponentInChildren<Text>();
        textGrid.text = text;
        debugCell.transform.position = GridManager.GetGrid().GetWorldPosition((int)gridPosition.x, (int)gridPosition.y);

    }
}
