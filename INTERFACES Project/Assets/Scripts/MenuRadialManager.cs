using UnityEngine;
using System.Collections.Generic;

public class MenuRadialManager : MonoBehaviour
{
    // Singleton pour pouvoir y accéder facilement depuis ton script d'interaction
    public static MenuRadialManager Instance;

    [Header("Visuel du Menu")]
    public GameObject conteneurFilterMedia;
    public GameObject conteneurNBSSArea;

    [Header("Paramètres des buissons")]
    public GameObject prefabShrub; // Glisse ton UNIQUE buisson ici
    public float decalageVersRoute = 2.5f;
    public float espacementEntreShrubs = 1.0f; // Distance (en mètres) entre chaque buisson

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
    public void OuvrirMenu(Transform positionCible, string objectId, string typeObjet)
    {
        Debug.Log("Le menu essaie de s'ouvrir pour l'objet : " + objectId);
        idObjetActuel = objectId; // On sauvegarde l'ID pour l'utiliser plus tard

        // On place le menu un peu au-dessus de l'objet
        // Change le 0.5f (50 centimètres) par une valeur plus grande, comme 2.0f (2 mètres) ou même 3.0f.
        //transform.position = positionCible.position + new Vector3(0, 3f, 0);
        conteneurFilterMedia.SetActive(false);
        conteneurNBSSArea.SetActive(false);

        if (typeObjet == "NBSS_area")
        {
            transform.position = positionCible.position + new Vector3(0, 1f, 0);
            conteneurNBSSArea.SetActive(true);
        }
        else if (typeObjet == "filter_media")
        {
            transform.position = positionCible.position + new Vector3(0, 3f, 0);
            conteneurFilterMedia.SetActive(true);
        }
    }

    public void FermerMenu()
    {
        conteneurFilterMedia.SetActive(false);
        conteneurNBSSArea.SetActive(false);
        idObjetActuel = ""; // On nettoie l'ID par sécurité
    }

    // créer barrière végétale
    

    public void CreerBarriereVeg(GameObject noueCliquee)
    {
        // --- 1. TROUVER LA ROUTE LA PLUS PROCHE ---
        GameObject[] toutesLesRoutes = GameObject.FindGameObjectsWithTag("road");
        if (toutesLesRoutes.Length == 0)
        {
            Debug.LogError("ERREUR : Aucun GameObject avec le tag 'road' trouvé !");
            return;
        }

        GameObject routePlusProche = null;
        float distanceMinimum = Mathf.Infinity;

        foreach (GameObject route in toutesLesRoutes)
        {
            float distance = Vector3.Distance(noueCliquee.transform.position, route.transform.position);
            if (distance < distanceMinimum)
            {
                distanceMinimum = distance;
                routePlusProche = route;
            }
        }

        // --- 2. CALCULER LA LONGUEUR ET L'ORIENTATION DE LA NOUE (CORRIGÉ) ---
        Collider col = noueCliquee.GetComponentInChildren<Collider>();
        float longueurNoue = 10f;

        Vector3 directionLigne = Vector3.right;    // Direction de la haie
        Vector3 directionDecalage = Vector3.forward; // Direction vers la route

        if (col != null)
        {
            float sizeX = col.bounds.size.x;
            float sizeZ = col.bounds.size.z;

            // On compare les axes pour savoir si la noue est horizontale ou verticale
            if (sizeX >= sizeZ)
            {
                // La noue est HORIZONTALE (plus longue sur l'axe X)
                longueurNoue = sizeX;
                directionLigne = Vector3.right; // La ligne de buissons va suivre l'axe X

                // La route est donc soit au Nord (+Z) soit au Sud (-Z)
                float signeZ = Mathf.Sign(routePlusProche.transform.position.z - noueCliquee.transform.position.z);
                directionDecalage = new Vector3(0, 0, signeZ);
            }
            else
            {
                // La noue est VERTICALE (plus longue sur l'axe Z)
                longueurNoue = sizeZ;
                directionLigne = Vector3.forward; // La ligne de buissons va suivre l'axe Z

                // La route est donc soit à l'Est (+X) soit à l'Ouest (-X)
                float signeX = Mathf.Sign(routePlusProche.transform.position.x - noueCliquee.transform.position.x);
                directionDecalage = new Vector3(signeX, 0, 0);
            }
        }

        // Le point central de la barrière, décalé parfaitement vers la route
        Vector3 centreBarriere = noueCliquee.transform.position + (directionDecalage * decalageVersRoute);

        // --- 3. SPAWN PARALLÈLE ---
        float moitie = longueurNoue / 2f;
        int compteur = 0;

        for (float d = -moitie; d <= moitie; d += espacementEntreShrubs)
        {
            // d avance le long de "directionLigne" (soit tout en X, soit tout en Z)
            Vector3 positionShrub = centreBarriere + (directionLigne * d);
            Quaternion rotationAleatoire = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Instantiate(prefabShrub, positionShrub, rotationAleatoire);
            compteur++;
        }

        Debug.Log($"SUCCÈS : {compteur} buissons plantés parallèlement à {noueCliquee.name}");
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
        EnvoyerCommandeGama("planter_flore_locale");
    }

    public void BoutonActionDesimpermeabiliser()
    {
        EnvoyerCommandeGama("desimpermeabiliser_sol");
    }

    public void BoutonActionPaillage()
    {
        EnvoyerCommandeGama("paillage");
    }

    public void BoutonActionBarriereVeg()
    {
        GameObject zoneAAmemenager = GameObject.Find(idObjetActuel);

        if (zoneAAmemenager != null)
        {
            CreerBarriereVeg(zoneAAmemenager);
            Debug.Log("barrière créée");
        }
        else
        {
            Debug.LogWarning("Impossible de retrouver la zone : " + idObjetActuel);
        }

        // 3. On ferme le menu
        FermerMenu();
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