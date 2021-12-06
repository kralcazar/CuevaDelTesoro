using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InputManager : MonoBehaviour
{
    public InputField inputGridSize;
    public InputField inputNumAgents;
    public Text prompt;

    public GameManager gameManager;
    public GameObject initCanvas;
    public GameObject game;
    public GameObject gameOver;

    private void Start()
    {
        inputGridSize.text = "6";
        inputNumAgents.text = "1";
    }

    public void StartGame()
    {
        bool isParsable;

        isParsable = int.TryParse(inputGridSize.text, out int gridSize);
        if (isParsable)
            GameManager.gridSize = gridSize;
        else
        {
            prompt.text = "Introduce un número para el tamaño del tablero.";
            Invoke(nameof(ResetPrompt), 5);
            return;
        }
            
        isParsable = int.TryParse(inputNumAgents.text, out int numAgents);
        if (isParsable)
            GameManager.numAgents = numAgents;
        else { 
            prompt.text = "Introduce un número para el número de agentes.";
            Invoke(nameof(ResetPrompt), 5);
            return;
        }

        initCanvas.SetActive(false);
        game.SetActive(true);

        GameManager.InitGame();
    }

    public void GameOver()
    {
        game.SetActive(false);
        gameOver.SetActive(true);
    }

    public void ResetGame()
    {
        gameManager.ResetInstance();
        gameOver.SetActive(false);
        game.SetActive(false);
        initCanvas.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void ResetPrompt()
    {
        prompt.text = "";
    }
}
