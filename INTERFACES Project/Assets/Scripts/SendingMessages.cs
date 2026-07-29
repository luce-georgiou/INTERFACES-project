using UnityEngine;
using TMPro;
using System.Collections;

public class SendingMessages : MonoBehaviour
{
    public static SendingMessages Instance;
    public TextMeshProUGUI text;

    private int prioriteActuelle = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //Hiding text at the beginning of the game
        if (text != null)
        {
            text.gameObject.SetActive(false);
        }
    }

    public static void Show(string msg, float duree = 7f, Color? couleur = null, int priorite = 0)
    {
        if (Instance != null && Instance.text != null)
        {

            if (Instance.text.gameObject.activeInHierarchy && priorite < Instance.prioriteActuelle)
            {
                return;
            }
            Instance.prioriteActuelle = priorite;
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.ShowForDuration(msg, duree, couleur ?? Color.black));
        }
    }

    //Coroutine handling showing/disappearing of message
    private IEnumerator ShowForDuration(string msg, float duree, Color couleur)
    {
        //Modify and display text
        text.text = msg;
        text.color = couleur;
        text.gameObject.SetActive(true);

        //Waiting for display time
        yield return new WaitForSeconds(duree);

        //Hide text
        text.gameObject.SetActive(false);

        prioriteActuelle = -1;
    }
}