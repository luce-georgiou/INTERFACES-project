using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class MenuRadialManager : MonoBehaviour
{
    // Singleton pour pouvoir y accéder facilement depuis ton script d'interaction
    public static MenuRadialManager Instance;

    [Header("Scenarios")]
    public Transform Scenario1;
    public Transform Scenario2;

    [Header("Visuel du Menu")]
    public GameObject conteneurFilterMedia;
    public GameObject conteneurNBSSArea;
    public GameObject SC1_Buttons;
    public GameObject SC2_Buttons;
    public GameObject conteneurArrosage;

    [Header("Matériaux")]
    [SerializeField] private Material burntGrassMat;
    [SerializeField] private Material healthyGrassMat;

    [Header("Paramètres des barrières")]
    public GameObject prefabSprout;
    public GameObject prefabMetalFence;
    public float decalageVersRoute = 2.5f;
    public float espacementEntreShrubs = 1.0f; // Distance (en mètres) entre chaque buisson
    public float spaceBetweenFences = 3.0f;

    [Header("Object spawner")]
    public GameObject prefabSign;
    public GameObject prefabTree;

    // Cette variable va mémoriser l'ID du filter_media sur lequel on a cliqué
    private string idObjetActuel = "";
    private float weight_score = 0.0f;
    // Mémorise l'heure exacte (en secondes) de la dernière mise à jour du score
    private float tempsDernierScore = 0f;

    [SerializeField] private ProgressBar progressBarObj;

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

    private void OnEnable()
    {
        //SimulationManagerInteraction.OnGAMAMessageReceived -= TreatMessage;
        SimulationManagerInteraction.OnGAMAMessageReceived += TreatMessage;
    }
    private void OnDisable()
    {
        SimulationManagerInteraction.OnGAMAMessageReceived -= TreatMessage;
    }
    private void TreatMessage(GAMAMessage2 mes)
    {
        //Debug.Log(mes);
        if (!string.IsNullOrWhiteSpace(mes.add_to_score))
        {
            if (Time.time - tempsDernierScore < 0.01f)
            {
                Debug.LogWarning("Doublon de score bloqué");
                return; // On arrête la lecture ici pour ce message précis
            }

            // On met à jour l'heure du dernier score pour le prochain coup
            tempsDernierScore = Time.time;

            Debug.Log("add to the score : " + mes.add_to_score);
            weight_score = float.Parse(mes.add_to_score, CultureInfo.InvariantCulture);
            progressBarObj.BarValue += weight_score;
            Dictionary<string, string> args = new Dictionary<string, string> {
                 {"id",ConnectionManager.Instance.GetConnectionId() },
                 {"mes",  progressBarObj.BarValue.ToString() }};
            ConnectionManager.Instance.SendExecutableAsk("receive_message", args);
        }
    }

    // On a ajouté le paramètre "objectId"
    public void OuvrirMenu(Transform positionCible, string objectId, string typeObjet)
    {
        if (!SimulationManagerInteraction.interactionsAutorisees) return;

        Debug.Log("Le menu essaie de s'ouvrir pour l'objet : " + objectId);
        idObjetActuel = objectId; // On sauvegarde l'ID pour l'utiliser plus tard

        // On place le menu un peu au-dessus de l'objet
        // Change le 0.5f (50 centimètres) par une valeur plus grande, comme 2.0f (2 mètres) ou même 3.0f.
        //transform.position = positionCible.position + new Vector3(0, 3f, 0);
        conteneurFilterMedia.SetActive(false);
        conteneurNBSSArea.SetActive(false);

        if (typeObjet == "NBSS_area")
        {
            //transform.position = positionCible.position; //+ new Vector3(0, 1f, 0);
            if (SimulationManagerInteraction.scenario == 1) conteneurNBSSArea.SetActive(false);
            else 
            {
                conteneurNBSSArea.SetActive(true);
                PlacerMenuProcheDuJoueur(this.gameObject, positionCible, new Vector3(0, 0.2f, 0));
            }
        }
        else if (typeObjet == "filter_media")
        {
            //transform.position = positionCible.position; //+ new Vector3(0, 3f, 0);
            conteneurFilterMedia.SetActive(true);
            PlacerMenuProcheDuJoueur(this.gameObject, positionCible, new Vector3(0, -0.3f, 0));
            if (SimulationManagerInteraction.scenario == 1)
            {
                SC1_Buttons.SetActive(true);
                SC2_Buttons.SetActive(false);
            }
            else if (SimulationManagerInteraction.scenario == 2)
            {
                SC1_Buttons.SetActive(false);
                SC2_Buttons.SetActive(true);
            }
        }
    }

    public void FermerMenu()
    {
        conteneurFilterMedia.SetActive(false);
        conteneurNBSSArea.SetActive(false);
        conteneurArrosage.SetActive(false);
        SC1_Buttons.SetActive(false);
        SC2_Buttons.SetActive(false);
        idObjetActuel = ""; // On nettoie l'ID par sécurité
    }

    // créer barrière végétale


    public void CreerBarriere(GameObject noueCliquee, GameObject fenceType)
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
        float moitie = (longueurNoue - 9f) / 2f;
        int compteur = 0;
        Debug.LogWarning(fenceType.name);
        bool isShrub = fenceType.name.Contains("Plant");
        Debug.LogWarning(isShrub);
        float spaceBetweenObjs = isShrub ? espacementEntreShrubs : spaceBetweenFences;

        for (float d = -moitie; d <= moitie; d += spaceBetweenObjs)
        {
            // d avance le long de "directionLigne" (soit tout en X, soit tout en Z)
            Vector3 positionShrub = centreBarriere + (directionLigne * d);
            Vector3 positionFinale = positionShrub;
            Quaternion rotationAlignee = Quaternion.LookRotation(directionLigne);
            if (isShrub)
            {
                rotationAlignee = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
            else
            {
                float decalageCorrectionPivot = 9.0f;

                positionFinale -= (directionLigne * decalageCorrectionPivot);
                fenceType.transform.localScale = new Vector3(fenceType.transform.localScale.x, 0.3f, fenceType.transform.localScale.z);
            }

            GameObject newFence = Instantiate(fenceType, positionFinale, rotationAlignee, Scenario2.transform);
            newFence.tag = "shrubs_plants";
            compteur++;
        }

        Debug.Log($"SUCCÈS : {compteur} buissons plantés parallèlement à {noueCliquee.name}");
    }

    public void PoserPaillage(GameObject noueCliquee)
    {
        Material mat = Resources.Load<Material>("Stylize Wood Texture/Materials/Stylize Wood ");
        noueCliquee.GetComponent<Renderer>().material = mat;

        int idx = int.Parse(noueCliquee.name.Replace("nbss_area", ""));
        GameObject fmObj = GameObject.Find("filter_media" + idx);

        Vector3 tailleFM = fmObj.GetComponent<Renderer>().bounds.size;
        GameObject paillageObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paillageObj.transform.SetParent(Scenario2, true);
        paillageObj.name = "paillage" + idx;
        paillageObj.tag = "paillage";
        paillageObj.transform.localScale = new Vector3(tailleFM.x, 0.2f, tailleFM.z);
        paillageObj.GetComponent<Renderer>().material = mat;
        paillageObj.transform.position = noueCliquee.transform.position + new Vector3(0, -2.3f, 0);
    }

    public void MowGrass(GameObject swale)
    {
        int idx = int.Parse(swale.name.Replace("nbss_area", ""));
        GameObject grassObj = GameObject.Find("Grass_NBSS" + idx);
        grassObj.SetActive(false);
        
    }

    public void ObjectSpawner(GameObject obj, Vector3 location, GameObject prefab)
    {
        Vector3 spawnLoc = obj.transform.position + location;
        if (prefab.name.Contains("Plant"))
        {
            GameObject newObject = Instantiate(prefab, spawnLoc, obj.transform.rotation * Quaternion.Euler(0f, 180f, 0f), Scenario2);
            newObject.tag = "sprout";
        }
        else
        {
            GameObject newObject = Instantiate(prefab, spawnLoc, obj.transform.rotation * Quaternion.Euler(0f, 90f, 0f), Scenario2);
        }

    }

    // --- LES BOUTONS DE TON MENU RADIAL ---
    // Associe ces fonctions à l'événement "On Click" des boutons de ton Canvas

    public void BoutonActionSensibilisation()
    {
        GameObject obj = GameObject.Find(idObjetActuel);
        Collider col = obj.GetComponent<Collider>();
        Vector3 loc = new Vector3(1.0f, 1.0f, 0f);
        if (col != null)
        {
            if (col.bounds.size.x > col.bounds.size.z)
            {
                // L'objet est plus large que long (Horizontal)
                loc = new Vector3(-1f, 0f, -3f);
                Debug.Log("Objet détecté comme Horizontal");
            }
            else
            {
                // L'objet est plus long que large (Vertical)
                loc = new Vector3(3f, 0f, 4f);
                Debug.Log("Objet détecté comme Vertical");
            }
        }
        ObjectSpawner(obj, loc, prefabSign);
        FermerMenu();
        progressBarObj.BarValue = progressBarObj.BarValue + 5f;
        SendingMessages.Show("La sensibilisation est la première ligne de défense de la noue.", 7f, Color.green);
        SimulationManager.Instance.SendMessageToGama(obj.name + ":" + "0");
    }

    public void BoutonActionPlanterArbre()
    {
        GameObject obj = GameObject.Find(idObjetActuel);
        Collider col = obj.GetComponent<Collider>();
        Vector3 loc = new Vector3(1.0f, 1.0f, 0f);
        if (col != null)
        {
            if (col.bounds.size.x > col.bounds.size.z)
            {
                // L'objet est plus large que long (Horizontal)
                loc = new Vector3(2f, 0f, 3f);
                Debug.Log("Objet détecté comme Horizontal");
            }
            else
            {
                // L'objet est plus long que large (Vertical)
                loc = new Vector3(-0.5f, 0f, col.bounds.size.z/2 -0.5f);
                Debug.Log("Objet détecté comme Vertical");
            }
        }
        ObjectSpawner(obj, loc, prefabTree);
        FermerMenu();
        progressBarObj.BarValue = progressBarObj.BarValue + 15f;
        SendingMessages.Show("Planter des arbres permet de diversifier les strates de cet espace, et apporter un peu d'ombre et de fraîcheur à la noue.", 7f, Color.green);
        SimulationManager.Instance.SendMessageToGama(obj.name + ":" + "3");
    }

    public void BoutonActionCurage()
    {
        EnvoyerCommandeGama("curage");
        //SimulationManagerInteraction.unclogged = true; //à modif car valable partout là ou enlever
    }

    public void BoutonActionFlowers()
    {
        EnvoyerCommandeGama("plant_flowers");
    }

    public void BoutonActionArroser()
    {
        // Remplace "amenager_noue" par le nom exact de l'action dans GAMA
        //EnvoyerCommandeGama("arroser");
        conteneurArrosage.SetActive(true);
        SC1_Buttons.SetActive(false);
        SC2_Buttons.SetActive(false);
        
    }

    public void BoutonActionReplanter()
    {
        EnvoyerCommandeGama("planter_flore_locale");
    }

    public void BoutonActionTondre()
    {
        GameObject swale = GameObject.Find(idObjetActuel);
        MowGrass(swale);

        FermerMenu();
        progressBarObj.BarValue = progressBarObj.BarValue - 15f;
        SendingMessages.Show("Tondre à ras détruit l'ombre naturelle du sol ! L'humidité s'évapore et la biodiversité fuit.", 7f, Color.red);
        SimulationManager.Instance.SendMessageToGama(swale.name + ":" + "-15");
    }

    public void BoutonActionPaillage()
    {
        GameObject zoneAPailler = GameObject.Find(idObjetActuel);
        if (zoneAPailler != null)
        {
            PoserPaillage(zoneAPailler);
        }
        else
        {
            Debug.LogError("impossible de poser paillage");
        }
        FermerMenu();
        //EnvoyerCommandeGama("point_gain");
        progressBarObj.BarValue = progressBarObj.BarValue + 15f;
        SendingMessages.Show("Le paillage protège le sol de la chaleur et garde l’humidité.", 7f, new Color(119f / 255f, 178f / 255f, 107f / 255f));
        SimulationManager.Instance.SendMessageToGama(zoneAPailler.name + ":" + "5");
    }

    public void BoutonActionBarriereVeg()
    {
        GameObject zoneAAmemenager = GameObject.Find(idObjetActuel);

        if (zoneAAmemenager != null)
        {
            CreerBarriere(zoneAAmemenager, prefabSprout);
            Debug.Log("barrière créée");
        }
        else
        {
            Debug.LogWarning("Impossible de retrouver la zone : " + idObjetActuel);
        }

        // 3. On ferme le menu
        FermerMenu();
        progressBarObj.BarValue = progressBarObj.BarValue + 20f;
        SendingMessages.Show("Les plantes locales protègent la noue du piétinement naturellement, et ralentissent le tassement de la terre.", 7f, new Color(119f / 255f, 178f / 255f, 107f / 255f));
        SimulationManager.Instance.SendMessageToGama(zoneAAmemenager.name + ":" + "3");
    }

    public void BoutonActionMetalFence()
    {
        GameObject zoneAAmemenager = GameObject.Find(idObjetActuel);

        if (zoneAAmemenager != null)
        {
            CreerBarriere(zoneAAmemenager, prefabMetalFence);
            Debug.Log("barrière créée");
        }
        else
        {
            Debug.LogWarning("Impossible de retrouver la zone : " + idObjetActuel);
        }

        // 3. On ferme le menu
        FermerMenu();
        progressBarObj.BarValue = progressBarObj.BarValue - 5f;
        SendingMessages.Show("Le grillage bloque les piétons, mais aussi la faune locale ! Privilégiez une barrière végétale (haie).", 7f, Color.yellow);
        SimulationManager.Instance.SendMessageToGama(zoneAAmemenager.name + ":" + "+10");
    }

    public void BoutonActionGazon()
    {
        GameObject noue = GameObject.Find(idObjetActuel);
        int idx = int.Parse(noue.name.Replace("filter_media", ""));
        GameObject grass = GameObject.Find("Grass_NBSS" + idx);
        //Material burntGrassMat = Resources.Load<Material>("Prefabs/Mess Maker Free/Sample Scene Assets/Material/Grass");
        Material yellowGrassMat = Resources.Load<Material>("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Materials/Grass_1");
        foreach (Transform enfant in grass.transform)
        {
            // Random.Range(0f, 100f) génère un nombre aléatoire entre 0 et 100
            // Si ce nombre est inférieur à ton pourcentage, on éteint l'objet
            if (Random.Range(0f, 100f) < 50f)
            {
                enfant.gameObject.SetActive(false);
            }
            else
            {
                Renderer rend = enfant.GetComponent<Renderer>();
                if (rend != null)
                {
                    //rend.material.color = new Color(166f / 255f, 153f / 255f, 34f / 255f);
                    rend.sharedMaterial = yellowGrassMat;
                }
            }
        }
        FermerMenu();
        progressBarObj.BarValue = progressBarObj.BarValue - 20f;
        SendingMessages.Show("Une pelouse classique demande trop d'eau. Il faut privilégier des plantes locales adaptées à la sécheresse !", 7f, Color.red);
        SimulationManager.Instance.SendMessageToGama("nbss_area" + idx + ":" + "-10");
        //SimulationManager.Instance.StartCoroutine(SimulationManager.Instance.ShowForDuration("Une pelouse classique demande trop d'eau. Il faut privilégier des plantes locales adaptées à la sécheresse !", 5f));
    }

    public void BoutonActionArroserMaintenant()
    {
        GameObject fm = GameObject.Find(idObjetActuel);
        int idx = int.Parse(fm.name.Replace("filter_media", ""));
        GameObject nbss_area = GameObject.Find("nbss_area" + idx);
        GameObject grass = GameObject.Find("Grass_NBSS" + idx);

        fm.GetComponent<Renderer>().material = burntGrassMat;
        nbss_area.GetComponent<Renderer>().material = burntGrassMat;
        Renderer[] tousLesRenderers = grass.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in tousLesRenderers)
        {
            if (rend != null)
            {
                rend.material.color = Color.black;
            }
        }

        conteneurArrosage.SetActive(false);
        progressBarObj.BarValue = progressBarObj.BarValue - 5f;
        SendingMessages.Show("Arroser en pleine canicule est un gaspillage : 60% de l'eau s'évapore avant de toucher les racines !", 7f, Color.red);
        SimulationManager.Instance.SendMessageToGama(nbss_area.name + ":" + "-10");
    }

    public void BoutonArroserPlusTardOuTot()
    {
        GameObject fm = GameObject.Find(idObjetActuel);
        int idx = int.Parse(fm.name.Replace("filter_media", ""));
        GameObject nbss_area = GameObject.Find("nbss_area" + idx);
        GameObject grass = GameObject.Find("Grass_NBSS" + idx);

        fm.GetComponent<Renderer>().material = healthyGrassMat;
        nbss_area.GetComponent<Renderer>().material = healthyGrassMat;
        Renderer[] tousLesRenderers = grass.GetComponentsInChildren<Renderer>();
        Color c = new Color(0, 58f / 255f, 0);
        foreach (Renderer rend in tousLesRenderers)
        {
            if (rend != null)
            {
                rend.material.color = c;
            }
        }

        EnvoyerCommandeGama("water_late_early");
        //conteneurArrosage.SetActive(false);
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
            //Debug.Log(weight_score);
            //progressBarObj.BarValue = progressBarObj.BarValue + weight_score;
            //Debug.Log("score curage ajouté : " + progressBarObj.BarValue);
            Debug.Log($"Action '{nomActionGama}' envoyée pour l'objet : {idObjetActuel}");
        }

        // On referme le menu après avoir cliqué
        FermerMenu();
    }


    // J'ai retiré le paramètre "recul" de la fonction, on n'en a plus besoin !
    private void PlacerMenuProcheDuJoueur(GameObject menu, Transform objetCible, Vector3 offset)
    {
        // On récupère la position du joueur pour savoir d'où il regarde
        Vector3 positionJoueur = Camera.main.transform.position;
        Collider colObjet = objetCible.GetComponentInChildren<Collider>();

        Vector3 positionFinale = objetCible.position;

        if (colObjet != null)
        {
            // 1. Calcul du point du collider le plus proche du joueur sur la longueur
            Vector3 pointPlusProche = colObjet.ClosestPoint(positionJoueur);

            // --- CENTRER DANS LA LARGEUR ---
            // On traduit ce point par rapport au centre de l'objet visé
            Vector3 pointLocal = objetCible.InverseTransformPoint(pointPlusProche);

            // On force la largeur à être au centre (0)
            if (objetCible.name == "filter_media0" || objetCible.name == "filter_media1" || objetCible.name == "filter_media2" || objetCible.name == "filter_media3")
            {
                pointLocal.z = 0;
            }
            else pointLocal.x = 0;

            // On re-transforme ce point modifié en coordonnées mondiales
            pointPlusProche = objetCible.TransformPoint(pointLocal);
            // -----------------------------------------

            // 2. Position finale : on ajoute simplement l'offset (la hauteur)
            positionFinale = pointPlusProche + offset;
        }
        else
        {
            // Sécurité si pas de collider
            positionFinale = objetCible.position + offset;
        }

        // 3. Appliquer la position
        Debug.Log(positionFinale);
        Debug.Log(menu.transform.position);
        menu.transform.position = positionFinale;

        // La rotation est maintenant entièrement gérée par ton script Billboard !
    }
}