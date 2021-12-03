using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InputManager : MonoBehaviour
{
    public GameObject initCanvas;
    public InputField inputGridSize;
    public InputField inputNumAgents;
    public Text prompt;

    public GameObject game;

    public void StartGame()
    {
        bool isParsable;

        isParsable = int.TryParse(inputGridSize.text, out int gridSize);
        if (isParsable)
            GameManager.gridSize = gridSize;
        else
        {
            prompt.text = "Introduce un n�mero para el tama�o del tablero.";
            Invoke(nameof(ResetPrompt), 5);
            return;
        }
            

        isParsable = int.TryParse(inputNumAgents.text, out int numAgents);
        if (isParsable)
            GameManager.numAgents = numAgents;
        else { 
            prompt.text = "Introduce un n�mero para el n�mero de agentes.";
            Invoke(nameof(ResetPrompt), 5);
            return;
        }

        initCanvas.SetActive(false);
        game.SetActive(true);

        GameManager.InitGame();
    }

    private void ResetPrompt()
    {
        prompt.text = "";
    }
}
