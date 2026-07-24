using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InterfaceButton : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject DisplayCanvas;
    public GameObject commandsPanel;
    public ProgressBar progressBar;

    void Start()
    {
        if (menuPanel != null)
        {
            DisplayCanvas.SetActive(false);
            menuPanel.SetActive(true);
            commandsPanel.SetActive(false);
        }
    }
    public void LaunchSc1()
    {
        menuPanel.SetActive(false);

        Debug.Log("Le scénario 1 commence !");

        string mes = "scenario1";
        Dictionary<string, string> args = new Dictionary<string, string> {
                    {"id",ConnectionManager.Instance.GetConnectionId() },
                    {"mes",  mes }};
        ConnectionManager.Instance.SendExecutableAsk("receive_message", args);

        DisplayCanvas.SetActive(true);
        
    }

    public void LaunchSc2()
    {
        menuPanel.SetActive(false);

        Debug.Log("Le scénario 2 commence !");

        string mes = "scenario2";
        Dictionary<string, string> args = new Dictionary<string, string> {
                 {"id",ConnectionManager.Instance.GetConnectionId() },
                 {"mes",  mes }};
        ConnectionManager.Instance.SendExecutableAsk("receive_message", args);

        DisplayCanvas.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Fermeture de l'application...");
        // Quitte le jeu (ne se voit qu'une fois le jeu exporté, pas dans l'éditeur)
        Application.Quit();
    }

    public void SkipPhase()
    {
        Debug.Log("Skip to next phase");

        SimulationManager.Instance.SendMessageToGama(progressBar.BarValue.ToString());

        string mes = "skip";
        Dictionary<string, string> args = new Dictionary<string, string> {
                {"id",ConnectionManager.Instance.GetConnectionId() },
                {"mes",  mes }};
        ConnectionManager.Instance.SendExecutableAsk("receive_message", args);
    }

    public void SeeCommands()
    {
        Debug.Log("Commands menu");

        menuPanel.SetActive(false);
        commandsPanel.SetActive(true);
    }

    public void OK()
    {
        Debug.Log("OK");

        menuPanel.SetActive(true);
        commandsPanel.SetActive(false);

    }
}
