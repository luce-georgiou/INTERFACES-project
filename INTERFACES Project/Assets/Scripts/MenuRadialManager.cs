using UnityEngine;
using System.Collections.Generic;

public class MenuRadialManager : MonoBehaviour
{
    // Singleton pour pouvoir y accéder facilement depuis ton script d'interaction
    public static MenuRadialManager Instance;

    [Header("Visuel du Menu")]
    public GameObject conteneurMenu;

    // Cette variable va mémoriser l'ID du filter_media sur lequel on a cliqué
    private string idObjetActuel = "";

    private void Awake()
    {
        // Configuration du Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        FermerMenu();
    }

    // On a ajouté le paramètre "objectId"
    public void OuvrirMenu(Transform positionCible, string objectId)
    {
        Debug.Log("Le menu essaie de s'ouvrir pour l'objet : " + objectId);
        idObjetActuel = objectId; // On sauvegarde l'ID pour l'utiliser plus tard

        // On place le menu un peu au-dessus de l'objet
        // Change le 0.5f (50 centimètres) par une valeur plus grande, comme 2.0f (2 mètres) ou même 3.0f.
        transform.position = positionCible.position + new Vector3(0, 3f, 0);
        conteneurMenu.SetActive(true);
    }

    public void FermerMenu()
    {
        conteneurMenu.SetActive(false);
        idObjetActuel = ""; // On nettoie l'ID par sécurité
    }

    // --- LES BOUTONS DE TON MENU RADIAL ---
    // Associe ces fonctions à l'événement "On Click" des boutons de ton Canvas

    public void BoutonActionCurage()
    {
        EnvoyerCommandeGama("curage");
    }

    public void BoutonActionArroser()
    {
        // Remplace "amenager_noue" par le nom exact de l'action dans GAMA
        EnvoyerCommandeGama("arroser");
    }

    public void BoutonActionReplanter()
    {
        // Remplace "ajouter_insectes" par le nom exact de l'action dans GAMA
        EnvoyerCommandeGama("planter_flore_locale");
    }

    // --- FONCTION UTILITAIRE CENTRALE ---
    // Évite de répéter le code de connexion pour chaque bouton
    private void EnvoyerCommandeGama(string nomActionGama)
    {
        if (!string.IsNullOrEmpty(idObjetActuel))
        {
            Dictionary<string, string> args = new Dictionary<string, string> {
                {"id", idObjetActuel }
            };

            // On envoie l'action à GAMA
            ConnectionManager.Instance.SendExecutableAsk(nomActionGama, args);
            Debug.Log($"Action '{nomActionGama}' envoyée pour l'objet : {idObjetActuel}");
        }

        // On referme le menu après avoir cliqué
        FermerMenu();
    }
}