using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InterfaceButton : MonoBehaviour
{
    public GameObject menuPanel;

    void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
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
    }

    public void QuitGame()
    {
        Debug.Log("Fermeture de l'application...");
        // Quitte le jeu (ne se voit qu'une fois le jeu exporté, pas dans l'éditeur)
        Application.Quit();
    }

    public void SkipPhase()
    {
        Debug.Log("Le bouton a été cliqué !");

        string mes = "skip";
        Dictionary<string, string> args = new Dictionary<string, string> {
                {"id",ConnectionManager.Instance.GetConnectionId() },
                {"mes",  mes }};
        ConnectionManager.Instance.SendExecutableAsk("receive_message", args);
    }
}
