using UnityEngine;

public class Enviroment
{
    public static bool[] GetPerception(Vector2 gridPosition)
    {
        //Hedor, Brisa, Resplandor, Golpe
        bool[] perception = { false, false, false, false };
        int x = (int)gridPosition.x;
        int y = (int)gridPosition.y;

        //Si hay un muro es recibe un golpe. Marcaremos las casillas fuera del área con la percepción Golpe.
        if (!GridManager.GetGrid().XYInGrid(x, y)) //La casilla está fuera del área (hay un muro).
        {
            perception[3] = true; //Golpe
            return perception;
        }

        //Ver si la casilla a consultar hay un tesoro
        CellType currentCellType = GridManager.GetGrid().GetGridObject(x, y).GetCellType();
        if (currentCellType == CellType.Tresor)
        {
            perception[2] = true; //Resplandor
        }

        //Mirar el conocimiento del entorno para indicar si hay efectos causados por casillas adyacentes
        int[] lookX = { -1, 0, 1, 0 };
        int[] lookY = { 0, -1, 0, 1 };
        
        
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = (int)gridPosition.x + lookX[i];
            int _y = (int)gridPosition.y + lookY[i];
            /*Debug.Log(new Vector2(_x, _y)) ;
            Debug.Log(GridManager.GetGrid().XYInGrid(_x, _y));*/
            if (!GridManager.GetGrid().XYInGrid(_x, _y)) //La casilla está fuera del área (hay un muro).
            {
                //perception[3] = true; //Golpe
                continue;
            }
            CellType surroundingCellType = GridManager.GetGrid().GetGridObject(_x, _y).GetCellType();
            switch (surroundingCellType)
            {
                case CellType.Cliff:
                    perception[1] = true; //Brisa
                    break;
                case CellType.Monster:
                    perception[0] = true; //Hedor
                    break;
            }
        }
        return perception;
    }
}