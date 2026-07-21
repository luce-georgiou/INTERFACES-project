using UnityEngine;
using TMPro;
using System.Collections; // Obligatoire pour utiliser les Coroutines (IEnumerator)

public class SendingMessages : MonoBehaviour
{
    public static SendingMessages Instance;
    public TextMeshProUGUI text;

    void Awake()
    {
        // Sécurité standard d'un Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // On cache le texte au démarrage du jeu par défaut
        if (text != null)
        {
            text.gameObject.SetActive(false);
        }
    }

    // La fonction statique que tu peux appeler de n'importe où !
    // J'ai ajouté un paramètre optionnel "duree" (par défaut 5 secondes)
    public static void Show(string msg, float duree = 7f, Color? couleur = null)
    {
        if (Instance != null && Instance.text != null)
        {
            // On arrête une éventuelle ancienne coroutine si un nouveau message arrive avant la fin de l'autre
            Instance.StopAllCoroutines();

            // On lance la coroutine SUR LE MANAGER (donc elle ne sera jamais détruite)
            Instance.StartCoroutine(Instance.ShowForDuration(msg, duree, couleur ?? Color.black));
            Debug.Log(msg);
        }
    }

    // La coroutine secrète qui gère l'apparition/disparition
    private IEnumerator ShowForDuration(string msg, float duree, Color couleur)
    {
        // 1. On modifie le texte et on l'affiche
        text.text = msg;
        text.color = couleur;
        text.gameObject.SetActive(true);

        // 2. On attend le temps défini
        yield return new WaitForSeconds(duree);

        // 3. On cache le texte à la fin
        text.gameObject.SetActive(false);
    }
}