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

    //<Cell, Visited>, las celdas visitadas por el agente
    private Dictionary<Vector2, bool> knowledgeVisited = new Dictionary<Vector2, bool>();

    //Meta
    private bool hasTresor;

    public void Inform(Vector2 gridPosition, bool[] knowledgeVector)
    {
        if (!knowledgePerceptions.ContainsKey(gridPosition))
        {
            knowledgePerceptions.Add(gridPosition, knowledgeVector);
        }
        else
        {
            knowledgePerceptions[gridPosition] = knowledgeVector;
        }
        /*
        for (int i = 0; i < knowledgePerceptions[gridPosition].Length; i++)
        {
            Debug.Log(knowledgePerceptions[gridPosition][i]);
        }
        */
    }

    public void InformAction(Vector2 gridPosition)
    {
        knowledgeVisited.Add(gridPosition, true);
    }

    public void InformTresor(Vector2 gridPosition)
    {
        hasTresor = true;
    }

    //Aplicar las reglas para inferir conocimiento (deductivo)
    public void InferCell(Vector2 gridPosition)
    {
        int[] lookX = { 0, -1, 0, 1, 0 };
        int[] lookY = { 0, 0, 1, 0, -1 };


        if (knowledgeVisited.ContainsKey(gridPosition))
        {
            SetKnowledgeInfered(gridPosition, CellType.Empty);
        }
        
        if (knowledgePerceptions.ContainsKey(gridPosition))
        {

            //Si hay resplandor hay un tesoro
            if (knowledgePerceptions[gridPosition][2])
            {
                SetKnowledgeInfered(gridPosition, CellType.Tresor);
            }
            //Si no hay ningún efecto las casillas adyacentes son seguras.
            if (!knowledgePerceptions[gridPosition][0] && !knowledgePerceptions[gridPosition][1] && !knowledgePerceptions[gridPosition][3])
            {
                for (int i = 0; i < lookX.Length; i++)
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    SetKnowledgeInfered(adjacentPosition, CellType.Empty);
                }
            }

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
                for (int i = 0; i < lookX.Length; i++) //Posible monstruo en las adyacentes
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
                for (int i = 0; i < lookX.Length; i++) //Posible acantilado en las adyacentes
                {
                    Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                    if (!noCliff.ContainsKey(adjacentPosition))
                    {
                        noCliff.Add(adjacentPosition, true);
                    }
                }
            }
        }


        // Si se sabe que no hay monstruo ni acantilado ni tesoro la celda está vacía
        if (noCliff.ContainsKey(gridPosition) && noMonster.ContainsKey(gridPosition) && knowledgeInfered.ContainsKey(gridPosition))
        {
            if (noCliff[gridPosition] && noMonster[gridPosition] && knowledgeInfered[gridPosition] != CellType.Tresor)
            {
                SetKnowledgeInfered(gridPosition, CellType.Empty);
            }
        }

        /*
        Debug.Log("knowledgePerceptions");
        foreach (var entry in knowledgePerceptions)
        {
            Debug.Log(entry.Key);
            for (int i = 0; i < entry.Value.Length; i++)
            {
                Debug.Log(entry.Value[i]);
            }
        }
        
        Debug.Log("knowledgeInfered");
        foreach (var entry in knowledgeInfered)
        {
            Debug.Log(entry.Key);
            Debug.Log(entry.Value);
        }*/
    }

    private void SetKnowledgeInfered(Vector2 gridPosition, CellType cellType)
    {
        if (!knowledgeInfered.ContainsKey(gridPosition))
        {
            if(GridManager.GetGrid().XYInGrid((int)gridPosition.x, (int)gridPosition.y))
            knowledgeInfered.Add(gridPosition, cellType);
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
        if (!hasTresor) //Si no tiene el tesoro (iterar por celdas seguras)
        {
            if (knowledgeVisited.ContainsKey(gridPosition)) return -1;

            Debug.Log("AskPriority: "+ gridPosition);
            if (knowledgeInfered.ContainsKey(gridPosition)) // Hay conocimiento sobre el estado de la celda
            {
                Debug.Log(knowledgeInfered[gridPosition]);
                if (knowledgeInfered[gridPosition] == CellType.Tresor) return 2;
                else if (knowledgeInfered[gridPosition] == CellType.Empty) return 1;
                else return -1;
            }
        }
        else //Si tiene el tesoro (volver por las celdas ok)
        {

        }


        return -1; //No conviene tomar esta casilla
    }
}