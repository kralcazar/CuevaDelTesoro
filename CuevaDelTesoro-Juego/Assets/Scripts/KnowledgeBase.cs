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

    public KnowledgeBase(Agent agent)
    {
        this.agent = agent;
    }

    public void Inform(Vector2 gridPosition, bool[] knowledgeVector)
    {
        if (!knowledgePerceptions.ContainsKey(gridPosition))
        {
            knowledgePerceptions.Add(gridPosition, knowledgeVector);
            Debug.Log("Inform: " + gridPosition + " : " + string.Join(",", knowledgePerceptions[gridPosition]));
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
        Debug.Log("InformVisited: " + gridPosition + " : "+ knowledgeVisited[gridPosition]);
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
            Debug.Log("INFER PERCEPTIONS: " + gridPosition + " : " + string.Join(",", knowledgePerceptions[gridPosition]));
            
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
                    //Hedor
                    if (knowledgePerceptions[adjacentPosition][0])
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
                    //Brisa
                    if (knowledgePerceptions[adjacentPosition][1])
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

        Debug.LogError(gridPosition+" : "+knowledgeVisited.ContainsKey(gridPosition));
        //Si la hemos visitado -> Empty
        if (knowledgeVisited.ContainsKey(gridPosition))
        {
            SetKnowledgeInfered(gridPosition, CellType.Empty);
            Debug.LogError(knowledgeVisited[gridPosition]);
        }
        //Si hay resplandor -> Tresor
        if (knowledgePerceptions[gridPosition][2])
        {
            SetKnowledgeInfered(gridPosition, CellType.Tresor);
        }


        Debug.Log("INFER KNOWLEDGE: " + gridPosition + " : " + string.Join(",", knowledgePerceptions[gridPosition]));
        //Si no hay ningún efecto -> las casillas adyacentes son seguras, se infiere Empty. ¡Sólo si la hemos percibido (podría ser un tesoro)!
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
                if (!knowledgePerceptions.ContainsKey(adjacentPosition)) continue; //Si no hemos percibido la casilla salimos (podría ser tesoro)
                if (knowledgePerceptions.ContainsKey(adjacentPosition)) if(knowledgePerceptions[adjacentPosition][2]) continue; //Si es tesoro salimos

                // Si la casilla no tiene ningún estado se infiere Empty
                SetKnowledgeInfered(adjacentPosition, CellType.Empty);
            }
        }



        if (knowledgeInfered.ContainsKey(gridPosition)) return; //Poda



        //Si en las casillas adyacentes hay hedor/brisa en esta casilla hay monstruo/acantilado        
        if (noMonster.ContainsKey(gridPosition)) //Si ya sabemos que no hay monstruo salimos
        {
            if (noMonster[gridPosition])

                goto NoMatchMonster;
        }
        for (int i = 0; i < lookX.Length; i++)
        {
            Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
            if (!knowledgePerceptions.ContainsKey(adjacentPosition)) //Si aún no hemos percibido esa casilla salimos
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


        //Si se sabe que hay pared saltamos
        if (knowledgeInfered.ContainsKey(gridPosition)) if (knowledgePerceptions[gridPosition][3] || knowledgePerceptions[gridPosition][2]) goto EndWall;
        // Si se sabe que no hay monstruo ni acantilado ni tesoro la celda está vacía
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
                Debug.Log("knowledgeInfered: "+ gridPosition + " : " + knowledgeInfered[gridPosition]);
                agent.ShowKnowledge(gridPosition, cellType.ToString());
            }
        }
    }

    //Si la posición preguntada es segura (reactivo)
    public bool Ask(Vector2 gridPosition)
    {
        //Si no tiene el tesoro (iterar por celdas seguras)

        //Si tiene el tesoro (volver por las celdas ok)

        return false;
    }

    //Devuelve la prioridad de la acción a tomar -1: No, 1: Poca prioridad, 2: Mucha prioridad (deductivo)
    public int AskPriority(Vector2 gridPosition)
    {
        if (knowledgePerceptions[gridPosition][3]) return int.MinValue; //Golpe

        if (!hasTresor) //Si no tiene el tesoro (iterar por celdas seguras)
        {
            Debug.Log("AskPriority: " + gridPosition);

            if (knowledgeVisited.ContainsKey(gridPosition))
            {
                Debug.LogError("Ask visited " + knowledgeVisited[gridPosition]);
                //Mínima prioridad para poder volver atrás por el camino visitado
                //Cuantas más visitas ha hecho menos prioridad tiene
                return -knowledgeVisited[gridPosition]; 
            }

            if (knowledgeInfered.ContainsKey(gridPosition)) // Hay conocimiento sobre el estado de la celda
            {
                Debug.Log("knowledgeInfered >> "+" : "+ gridPosition + knowledgeInfered[gridPosition]);
                if (knowledgeInfered[gridPosition] == CellType.Tresor) return 2;
                else if (knowledgeInfered[gridPosition] == CellType.Empty) return 1;
            }
        }
        else //Si tiene el tesoro (volver por las celdas ok)
        {

        }


        return int.MinValue; //No conviene tomar esta casilla
    }
}