using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KnowledgeBase
{
    // Zona de hechos para inferir conocimiento
    //<Cell, Knowledge (Hedor, Brisa, Resplandor, Golpe)>
    private Dictionary<Vector2, bool[]> knowledgePerceptions = new Dictionary<Vector2, bool[]>();

    private Dictionary<Vector2, bool> posibleMonster = new Dictionary<Vector2, bool>();
    private Dictionary<Vector2, bool> posibleCliff = new Dictionary<Vector2, bool>();
    private Dictionary<Vector2, bool> noMonster = new Dictionary<Vector2, bool>();
    private Dictionary<Vector2, bool> noCliff = new Dictionary<Vector2, bool>();

    // Conocimiento inferido, se conoce con seguridad el tipo de celda 
    private Dictionary<Vector2, CellType> knowledgeInfered = new Dictionary<Vector2, CellType>();

    //<Cell, Visited>, Las visitas realizadas en cada celda por el agente
    private Dictionary<Vector2, int> knowledgeVisited = new Dictionary<Vector2, int>();

    //Meta
    private bool hasTresor;

    private Agent agent;
    private Cell previousVisited;

    private int numMovements;

    public KnowledgeBase(Agent agent)
    {
        this.agent = agent;
    }

    public void Inform(Vector2 gridPosition, bool[] knowledgeVector)
    {
        if (!knowledgePerceptions.ContainsKey(gridPosition))
        {
            knowledgePerceptions.Add(gridPosition, knowledgeVector);
            //Debug.Log("Inform: " + gridPosition + " : " + string.Join(",", knowledgePerceptions[gridPosition]));
        }
        else
        {
            knowledgePerceptions[gridPosition] = knowledgeVector;
        }
    }

    public void InformAction(Vector2 gridPosition)
    {
        if (!knowledgeVisited.ContainsKey(gridPosition))
        {
            knowledgeVisited.Add(gridPosition, 1);
        }
        else
        {
            knowledgeVisited[gridPosition]++;
        }
        numMovements++;
    }

    public void InformTresor(Vector2 gridPosition)
    {
        hasTresor = true;
    }

    int[] lookX = { -1, 0, 1, 0 };
    int[] lookY = { 0, 1, 0, -1 };
    //Aplicar las reglas para inferir conocimiento (deductivo)
    public void InferPerceptionRules(Vector2 gridPosition)
    {        
        if (knowledgePerceptions.ContainsKey(gridPosition))
        {
            //Debug.Log("INFER PERCEPTIONS: " + gridPosition + " : " + string.Join(",", knowledgePerceptions[gridPosition]));
            
            if (knowledgePerceptions[gridPosition][0]) //Hedor
            {
                for (int i = 0; i < lookX.Length; i++) //Posible monstruo en las adyacentes
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    if (!posibleMonster.ContainsKey(adjacentPosition))
                    {
                        posibleMonster.Add(adjacentPosition, true);
                    }
                }
            }
            else
            {
                for (int i = 0; i < lookX.Length; i++) //No hay monstruo en las adyacentes
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    if (!noMonster.ContainsKey(adjacentPosition))
                    {
                        noMonster.Add(adjacentPosition, true);
                    }
                }
            }

            if (knowledgePerceptions[gridPosition][1]) //Brisa
            {
                for (int i = 0; i < lookX.Length; i++) //Posible acantilado en las adyacentes
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    if (!posibleCliff.ContainsKey(adjacentPosition))
                    {
                        posibleCliff.Add(adjacentPosition, true);
                    }
                }
            }
            else
            {
                for (int i = 0; i < lookX.Length; i++) //No hay acantilado en las adyacentes
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    if (!noCliff.ContainsKey(adjacentPosition))
                    {
                        noCliff.Add(adjacentPosition, true);
                    }
                }
            }

            //Si en las casillas adyacentes no hay hedor en esta casilla no hay monstruo
            if (!noMonster.ContainsKey(gridPosition)) {
                for (int i = 0; i < lookX.Length; i++) //Posible acantilado en las adyacentes
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    if (!knowledgePerceptions.ContainsKey(adjacentPosition)) 
                        goto EndMonster;
                    //Hedor y no golpe
                    if (knowledgePerceptions[adjacentPosition][0] && !knowledgePerceptions[adjacentPosition][3])
                        goto EndMonster;
                }
                noMonster.Add(gridPosition, true);
                EndMonster:;
            }

            //Si en las casillas adyacentes no hay brisa en esta casilla no hay acantilado
            if (!noCliff.ContainsKey(gridPosition))
            {
                for (int i = 0; i < lookX.Length; i++) //Posible acantilado en las adyacentes
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    if (!knowledgePerceptions.ContainsKey(adjacentPosition)) 
                        goto EndCliff;
                    //Brisa y no golpe
                    if (knowledgePerceptions[adjacentPosition][1] && !knowledgePerceptions[adjacentPosition][3])
                        goto EndCliff;
                }
                noCliff.Add(gridPosition, true);
                EndCliff:;
            }

        }
        /*
        Debug.Log("knowledgePerceptions");
        foreach (var entry in knowledgePerceptions)
        {
            Debug.Log(entry.Key);
            Debug.Log(string.Join(",", entry.Value)); 
        }
        */
    }

    public void InferKnowledgeRules(Vector2 gridPosition)
    {
        //Si la hemos visitado -> Empty
        if (knowledgeVisited.ContainsKey(gridPosition))
        {
            SetKnowledgeInfered(gridPosition, CellType.Empty);
        }
        //Si hay resplandor -> Tresor
        if (knowledgePerceptions[gridPosition][2])
        {
            SetKnowledgeInfered(gridPosition, CellType.Tresor);
        }


        //Debug.Log("INFER KNOWLEDGE: " + gridPosition + " : " + string.Join(",", knowledgePerceptions[gridPosition]));
        //Si no hay ning?n efecto -> las casillas adyacentes son seguras, se infiere Empty. ?S?lo si la hemos percibido (podr?a ser un tesoro)!
        if (!knowledgePerceptions[gridPosition][0] && 
            !knowledgePerceptions[gridPosition][1] && 
            !knowledgePerceptions[gridPosition][3])
        {
            for (int i = 0; i < lookX.Length; i++)
            {
                Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                if (knowledgeInfered.ContainsKey(adjacentPosition)) continue; //Poda
                if (posibleCliff.ContainsKey(adjacentPosition)) if(posibleCliff[adjacentPosition]) continue; //CLIFF
                if (posibleMonster.ContainsKey(adjacentPosition)) if(posibleMonster[adjacentPosition]) continue; //MONSTER
                if (!knowledgePerceptions.ContainsKey(adjacentPosition)) continue; //Si no hemos percibido la casilla salimos (podr?a ser tesoro)
                if (knowledgePerceptions.ContainsKey(adjacentPosition)) if(knowledgePerceptions[adjacentPosition][2]) continue; //Si es tesoro salimos

                // Si la casilla no tiene ning?n estado se infiere Empty
                SetKnowledgeInfered(adjacentPosition, CellType.Empty);
            }
        }



        if (knowledgeInfered.ContainsKey(gridPosition)) return; //Poda

        //Comprobar la casilla que puede tener un monstruo
        if (posibleMonster.ContainsKey(gridPosition))
        {
            for (int i = 0; i < lookX.Length; i++)
            {
                Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                if (!knowledgePerceptions.ContainsKey(adjacentPosition))
                    goto NoMatchPMonster;

                //Hedor o golpe
                if (!(knowledgePerceptions[adjacentPosition][0] || knowledgePerceptions[adjacentPosition][3]))
                    goto NoMatchPMonster;
            }
            SetKnowledgeInfered(gridPosition, CellType.Monster);
            return;
        NoMatchPMonster:;
        }
        //Comprobar la casilla que puede tener un acantilado
        if (posibleCliff.ContainsKey(gridPosition))
        {
            for (int i = 0; i < lookX.Length; i++)
            {
                Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                if (!knowledgePerceptions.ContainsKey(adjacentPosition))
                {
                    //TODO: Deducir golpe si no podemos percibir la casilla
                    goto NoMatchPCliff;
                }

                //Brisa o golpe
                if (!(knowledgePerceptions[adjacentPosition][1] || knowledgePerceptions[adjacentPosition][3]))
                    goto NoMatchPCliff;
            }
            SetKnowledgeInfered(gridPosition, CellType.Cliff);
            return;
        NoMatchPCliff:;
        }

        //Si en las casillas adyacentes hay hedor/brisa en esta casilla hay monstruo/acantilado        
        if (noMonster.ContainsKey(gridPosition)) //Si ya sabemos que no hay monstruo salimos
        {
            if (noMonster[gridPosition])

                goto NoMatchMonster;
        }
        for (int i = 0; i < lookX.Length; i++)
        {
            Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
            if (!knowledgePerceptions.ContainsKey(adjacentPosition)) //Si a?n no hemos percibido esa casilla salimos
                goto NoMatchMonster;

            //Hedor o golpe
            if (!(knowledgePerceptions[adjacentPosition][0] || knowledgePerceptions[adjacentPosition][3]))
                goto NoMatchMonster;
        }
        //Hemos detectado hedor en las casilla adyacentes
        SetKnowledgeInfered(gridPosition, CellType.Monster);
        return;
        NoMatchMonster:;
        if (noCliff.ContainsKey(gridPosition))//Si ya sabemos que no hay acantilado salimos
        {
            if (noCliff[gridPosition])
                goto NoMatchCliff;
        }
        for (int i = 0; i < lookX.Length; i++)
        {
            Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
            if (!knowledgePerceptions.ContainsKey(adjacentPosition))
                goto NoMatchCliff;

            //Brisa o golpe
            if (!(knowledgePerceptions[adjacentPosition][1] || knowledgePerceptions[adjacentPosition][3]))
                goto NoMatchCliff;
        }
        //Hemos detectado brisa en las casilla adyacentes
        SetKnowledgeInfered(gridPosition, CellType.Cliff);
        return;
        NoMatchCliff:;


        //Si se sabe que hay pared o tesoro saltamos
        if (knowledgeInfered.ContainsKey(gridPosition)) if (knowledgePerceptions[gridPosition][3] || knowledgePerceptions[gridPosition][2]) goto EndWall;
        // Si se sabe que no hay monstruo ni acantilado la celda est? vac?a
        if (noCliff.ContainsKey(gridPosition) && noMonster.ContainsKey(gridPosition))
        {
            if (noCliff[gridPosition] && noMonster[gridPosition])
            {
                SetKnowledgeInfered(gridPosition, CellType.Empty);
                return;
            }
        }
        EndWall:;
    }

    private void SetKnowledgeInfered(Vector2 gridPosition, CellType cellType)
    {
        if (!knowledgeInfered.ContainsKey(gridPosition))
        {
            if (GridManager.GetGrid().XYInGrid((int)gridPosition.x, (int)gridPosition.y))
            {
                knowledgeInfered.Add(gridPosition, cellType);
                //Debug.Log("knowledgeInfered: "+ gridPosition + " : " + knowledgeInfered[gridPosition]);
                agent.ShowKnowledge(gridPosition, cellType.ToString());
            }
        }
    }

    //Si la posici?n preguntada es segura (reactivo)
    public bool Ask(Vector2 gridPosition)
    {
        //Si no tiene el tesoro (iterar por celdas seguras)

        //Si tiene el tesoro (volver por las celdas ok)

        return false;
    }

    //Devuelve la prioridad de la acci?n a tomar MinValue: No, [MinValue..MaxValue]: M?nima prioridad a M?xima prioridad (deductivo)
    public int AskPriority(Vector2 gridPosition)
    {
        if (knowledgePerceptions[gridPosition][3]) return int.MinValue; //Golpe

        if (!hasTresor) //Si no tiene el tesoro (iterar por celdas seguras)
        {
            //Debug.Log("AskPriority: " + gridPosition);

            if (knowledgeVisited.ContainsKey(gridPosition))
            {
                //Debug.LogError("Ask visited " + knowledgeVisited[gridPosition]);
                //M?nima prioridad para poder volver atr?s por el camino visitado
                //Cuantas m?s visitas ha hecho menos prioridad tiene
                return -knowledgeVisited[gridPosition]; 
            }

            if (knowledgeInfered.ContainsKey(gridPosition)) // Hay conocimiento sobre el estado de la celda
            {
                //Debug.Log("knowledgeInfered >> "+" : "+ gridPosition + knowledgeInfered[gridPosition]);
                if (knowledgeInfered[gridPosition] == CellType.Tresor) return 2;
                else if (knowledgeInfered[gridPosition] == CellType.Empty) return 1;
            }
        }
        else //Si tiene el tesoro (volver por las celdas ok)
        {
            if (knowledgeInfered.ContainsKey(gridPosition))
            {
                //Si no es una casilla vacia no es segura
                if (knowledgeInfered[gridPosition] != CellType.Empty) return int.MinValue;

                if (agent.GetStartGridPosition() == gridPosition)
                    return int.MaxValue;

                //M?nima prioridad para poder volver atr?s por el camino visitado
                //Cuantas m?s visitas ha hecho menos prioridad tiene
                int gridSize = GridManager.GetGrid().GetWidth();
                float maxDistance = DistanceToStart(new Vector2(gridSize, gridSize));

                //Para desempatar por el camino m?s visitado
                int scoreVisited = 0;
                if (knowledgeVisited.ContainsKey(gridPosition))
                    scoreVisited = 10 * (numMovements / knowledgeVisited[gridPosition]);

                //Para desempatar por la distancia m?s corta
                int scoreDistance = Mathf.RoundToInt(10*(maxDistance/DistanceToStart(gridPosition)));
                //Debug.LogWarning("scoreDistance:"+ gridPosition+" : " + scoreDistance);

                //Si la casilla a la que vamos a ir la hemos visitado justo antes reduce prioridad (prevenir bucles)
                if (GridManager.GetGrid().GetGridObject((int)gridPosition.x, (int)gridPosition.y) == previousVisited)
                    scoreVisited -= 5;

                return scoreVisited + scoreDistance;
            }
        }
        return int.MinValue; //No conviene tomar esta casilla
    }

    public void SetPreviousCellVisited(int x, int y)
    {
        previousVisited = GridManager.GetGrid().GetGridObject(x, y);
    }

    public bool HasTresor()
    {
        return hasTresor;
    }

    public int GetMovements()
    {
        return numMovements;
    }

    private float DistanceToStart(Vector2 gridPosition)
    {
        return Vector2.Distance(agent.GetStartGridPosition(), gridPosition);
    }
}