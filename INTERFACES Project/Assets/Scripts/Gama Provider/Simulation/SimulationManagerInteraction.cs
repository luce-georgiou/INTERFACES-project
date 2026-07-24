using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit; 
using UnityEngine.InputSystem;
using System.Linq;

using DigitalRuby.RainMaker;

using TMPro;
using System.Collections;

using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;




public class SimulationManagerInteraction : SimulationManager
{

    public static bool interactionsAutorisees = true;
    public static int scenario;
    public GameObject Scenario1;
    public GameObject Scenario2;
    public Material defaultSky;
    public Material summerSky;
    public GameObject DisplayCanvas;
    public GameObject menuPanel;
    public TMP_Text timerText;
    public TMP_Text dateTimeText;
    private Coroutine timerCoroutine;
    public ProgressBar progressBar;
    GAMAMessage2 message = null;
    private List<Attributes> lastAttributes;

    // Mettre dans cette liste tous les objets statiques
    private List<string> tagsToIgnore = new List<string> { "swale", "gravel", "grass", "flower", "NBSS", "road", "building", "park" };

    private Dictionary<string, GameObject> sedimentMap = new Dictionary<string, GameObject>();
    
    // for sc1 ponding areas
    private bool unblocked = false;
    public static bool unclogged = false;

    IEnumerator CountDown(int duration)
    {
        Debug.Log("CountDown started: " + duration);

        if (timerText == null)
        {
            Debug.LogError("timerText is null!");
            yield break;
        }

        int remaining = duration;
        while (remaining > 0)
        {
            // Mise à jour de l'affichage (ex: 3:00)
            timerText.text = remaining / 60 + ":" + (remaining % 60 < 10 ? "0" : "") + remaining % 60;

            // On attend 1 seconde
            yield return new WaitForSeconds(1f);

            remaining--;
        }

        // Fin du chrono
        timerText.text = "0:00";
    }

    



    //Defines what happens when a ray passes over an object 
    protected override void HoverEnterInteraction(HoverEnterEventArgs ev)
    {
         GameObject obj = ev.interactableObject.transform.gameObject;
        if (obj.tag.Equals("trash") || obj.tag.Equals("weeds"))
        {
            SimulationManagerSolo.ChangeColor(obj, Color.blue);
            SendingMessages.Show("Ces déchets et végétaux semblent obstruer la canalisation...\nNettoyer ?");
        }
        if (obj.tag.Equals("filter_media"))
        {
            SimulationManagerSolo.ChangeColor(obj, Color.blue);
            if (scenario == 1) SendingMessages.Show("Parfois, les sédiments s'accumulent au fond des noues, la bouchant. Veux-tu essayer de curer ?");
            if (scenario == 2) SendingMessages.Show("Sans eau et avec le piétinement, ces plantes n’ont aucune chance.");
        }
        if (obj.tag.Equals("nbss_area"))
        {
            SimulationManagerSolo.ChangeColor(obj, Color.blue);
            if (scenario == 2) SendingMessages.Show("Le sol est si sec qu’il ne peut plus retenir l’eau. Sans pluie, la noue va disparaître.");
        }
    }


    //Defines what happens when a ray passes not anymore over an object 
    protected override void HoverExitInteraction(HoverExitEventArgs ev)
    {
        GameObject obj = ev.interactableObject.transform.gameObject;
        if (obj.tag.Equals("trash") || obj.tag.Equals("weeds") || obj.tag.Equals("nbss_area") || obj.tag.Equals("filter_media"))
        {
            SimulationManagerSolo.ChangeColor(obj, Color.white);
            SendingMessages.Show("");
        }
    }

    //Defines what happens when a object is selected
    protected override void SelectInteraction(SelectEnterEventArgs ev)
    {
        if (!interactionsAutorisees) return;

        if (remainingTime <= 0.0)
        {
            GameObject grabbedObject = ev.interactableObject.transform.gameObject;
            Debug.Log("grabbed: " + grabbedObject.name + " tag: " + grabbedObject.tag);
            //Debug.Log("grabbedObject : " + grabbedObject);
            //int count = GameObject.FindGameObjectsWithTag("weeds").Length + GameObject.FindGameObjectsWithTag("trash").Length;
            
            float weight = 45f;
            if (grabbedObject.tag.Equals("weeds") || grabbedObject.tag.Equals("trash"))
            {
                //SendingMessages.Show("Moins de déchets = moins de pollution pour l’eau et le sol.");
                //StartCoroutine(Wait());
                Dictionary<string, string> args = new Dictionary<string, string> {
                         {"id", grabbedObject.name }
                    };
                ConnectionManager.Instance.SendExecutableAsk("maintenance_remove", args);
                progressBar.BarValue = progressBar.BarValue + weight;
                SendMessageToGama(progressBar.BarValue.ToString());
                unblocked = true;
                
                //count = GameObject.FindGameObjectsWithTag("weeds").Length + GameObject.FindGameObjectsWithTag("trash").Length;
                //if (count > 0)
                //{
                //    Debug.Log("count : " + count);
                //    //count -= 1;
                //    ConnectionManager.Instance.SendExecutableAsk("maintenance_remove", args);
                //    progressBar.BarValue = progressBar.BarValue + (weight / totalWeedsTrashInitial);
                //    SendMessageToGama(progressBar.BarValue.ToString());
                //    if (count == 1 && scenario == 1)
                //    {
                //        unblocked = true;
                //        StartCoroutine(ShowForDuration("Moins de déchets = moins de pollution pour l’eau et le sol.", 5f));
                //    }
                //}

            }
            else if (grabbedObject.tag.Equals("filter_media"))
            {
                // On ouvre le menu radial en lui passant la position de l'objet ET son ID
                if (scenario == 1)
                {
                    /* Action curage envoyée à GAMA */
                    Dictionary<string, string> args = new Dictionary<string, string>
                    {{"id", grabbedObject.name } };
                    ConnectionManager.Instance.SendExecutableAsk("curage", args);

                    //// On désactive les indications de défaillance liées aux sédiments
                    //if (score_curage > 0) 
                    //{
                    //    Debug.Log(score_curage);
                    //    unclogged = true;
                    //    GameObject[] empties = FindObjectsOfType<GameObject>()
                    //            .Where(go => go.name.StartsWith("EmptyPond"))
                    //            .ToArray();
                    //    foreach (GameObject pond in empties)
                    //    {
                    //        pond.SetActive(false);
                    //    }
                    //}
                }
                if (scenario == 2)
                {
                    MenuRadialManager.Instance.OuvrirMenu(grabbedObject.transform, grabbedObject.name, "filter_media");
                }
            }
            else if (grabbedObject.tag.Equals("nbss_area")) {
                MenuRadialManager.Instance.OuvrirMenu(grabbedObject.transform, grabbedObject.name, "NBSS_area");
                SendingMessages.Show("Moins de déchets = moins de pollution pour l’eau et le sol.");
            }
        }

    }

    // To find vertices of objects
    //Vector3[] GetNBSSVertices(string nbssName)
    //{
    //    GameObject[] nbssObjects = GameObject.FindGameObjectsWithTag("park");
    //    foreach (GameObject obj in nbssObjects)
    //    {
    //        if (obj.name == nbssName)
    //        {
    //            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
    //            if (mesh == null) return null;

    //            Vector3[] worldVertices = new Vector3[mesh.vertices.Length];
    //            for (int i = 0; i < mesh.vertices.Length; i++)
    //            {
    //                worldVertices[i] = obj.transform.TransformPoint(mesh.vertices[i]);
    //            }
    //            return worldVertices;
    //        }
    //    }
    //    return null;
    //}

    
    
    protected override void ManageAttributes(List<Attributes> attributes)
    {
        /* Afficher vertices gameobjects */
        //for (int i = 0; i < 8; i++)
        //{
        //    string nbs_name = "park" + i;
        //    Vector3[] vertices = GetNBSSVertices(nbs_name);
        //    if (vertices != null)
        //    {
        //        foreach (Vector3 v in vertices)
        //        {
        //            Debug.Log(nbs_name);
        //            Debug.Log(v);
        //        }
        //    }
        //}

        lastAttributes = attributes;

        for (int i = 0; i < infoWorld.names.Count; i++)
        {
            string name = infoWorld.names[i];

            if (tagsToIgnore.Any(tag => name.StartsWith(tag)))
                continue;

            List<object> o = geometryMap[name];
            GameObject obj = (GameObject)o[0];

            if (name.StartsWith("rain"))
            {
                float intensity = attributes[i].rain_intensity;
                string season = attributes[i].rain_seasons;
                BaseRainScript rainScript = GameObject.FindWithTag("rain").GetComponent<BaseRainScript>();
                ParticleSystem ps = rainScript.RainFallParticleSystem;

                var ma = ps.main;

                rainScript.RainIntensity = intensity / 3f;
                

                foreach (var key in geometryMap.Keys)
                {
                    if (!key.StartsWith("ponding_area")) continue;

                    GameObject pondObj = (GameObject)geometryMap[key][0];

                }
            }
            else if (name.StartsWith("filter_media"))
            {
                //InteractionLayerMask layerMask = InteractionLayerMask.GetMask("UndergroundObjects");

                // 2. Chercher le composant interactif XR sur cet objet (ou ses enfants)
                //UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = obj.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

                //if (interactable != null)
                //{
                //    // 3. Assigner le mask d'interaction dynamiquement
                //    interactable.interactionLayers = layerMask;
                //    //Debug.Log($"Layer d'interaction appliqué avec succès sur l'objet généré par GAMA : {obj.name}");
                //}
                //else
                //{
                //    Debug.LogWarning($"Aucun composant XRBaseInteractable trouvé sur l'objet GAMA : {obj.name}");
                //}

                obj.transform.position = new Vector3(obj.transform.position.x, -3f, obj.transform.position.z);
                Material sourceMat = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Ground02");
                
                Material mat = new Material(sourceMat);
                Color c = mat.GetColor("_BaseColor");

                if (name == "filter_media5")
                {

                    mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.SetFloat("_Surface", 1f);
                    
                    mat.SetFloat("_Blend", 0f); // Alpha blend mode
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = 3000;
                    //mat.SetFloat("_Alpha", 0.5f);

                    c.a = 0.6f;

                    //Debug.Log(c.a);
                    mat.SetColor("_BaseColor", c);
                    obj.GetComponent<Renderer>().material = mat;
                }
                

                //creating sediment accumulation
                int idx = int.Parse(obj.name.Replace("filter_media", ""));
                if (!sedimentMap.ContainsKey("SedimentAcc" + idx))
                {
                    GameObject sed_obj = GameObject.Find("SedimentAcc" + idx);
                    if (sed_obj != null)
                        sedimentMap["SedimentAcc" + idx] = sed_obj;
                }

                GameObject sedimentObj = sedimentMap.ContainsKey("SedimentAcc" + idx)
                    ? sedimentMap["SedimentAcc" + idx]
                    : null;

                int fqt = attributes[i].fqt_fm;
                if (fqt <= 1)
                {
                    
                   
                    
                    sedimentObj.SetActive(true);
                    sedimentObj.transform.position = new Vector3(sedimentObj.transform.position.x, -1.4f, sedimentObj.transform.position.z);
                    float sediment_acc = attributes[i].sediments_fm;
                    sedimentObj.transform.localScale = new Vector3(sedimentObj.transform.localScale.x, sediment_acc, sedimentObj.transform.localScale.z);
                }
                else
                {
                    sedimentObj.SetActive(false);
                }
            }
            else if (name.StartsWith("weeds") || name.StartsWith("trash"))
            {
                obj.transform.position = new Vector3(obj.transform.position.x, -1.6f, obj.transform.position.z);
            }
            else if (name.StartsWith("nbss_area"))
            {

                obj.transform.position = new Vector3(obj.transform.position.x, 0.8f, obj.transform.position.z);
                char index = name[name.Length - 1];
                if (index == '0' || index == '1') //|| index == '2' || index == '3')
                {
                    obj.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                }
                switch (index)
                {
                    //case '0':
                    //    obj.transform.localScale = new Vector3(0.8f, 1.2f, 1.02f);
                    //    break;
                    //case '1':
                    //    obj.transform.localScale = new Vector3(0.8f, 1.2f, 1.1f);
                    //    break;
                    case '0':
                    case '1':
                        obj.transform.localScale = new Vector3(1f, 1.2f, 2.56f);
                        break;
                    case '2':
                        obj.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
                        break;
                    case '3':
                        obj.transform.localScale = new Vector3(0.9f, 1.2f, 0.7f);
                        break;
                    //case '6':
                    //    obj.transform.localScale = new Vector3(0.9f, 1.2f, 0.6f);
                    //    break;
                    //case '7':
                    //    obj.transform.localScale = new Vector3(0.9f, 1.2f, 1.05f);
                    //    break;
                }
                float health = attributes[i].health;
                if (healthDic.ContainsKey(obj.name))
                {
                    healthDic[obj.name] = health;
                    //Debug.Log(obj.name);
                }
                else
                {
                    healthDic.Add(obj.name, health);
                    Debug.Log(obj.name + " with : " + health);
                }
            }
            else if (name.StartsWith("ponding_area"))
            {
                Material mat = Resources.Load<Material>("Materials/Water2/WaterVoronoi");
                mat.SetFloat("_Alpha", 0.5f);
                Renderer rend = obj.GetComponent<Renderer>();
                rend.material = mat;

                // if name = ponding_area2 and unblocked
                if (obj.name == "ponding_area0" && unblocked)
                {
                    rend.material.SetVector("_Direction_1", new Vector2(-1f, 0f));
                }

                //if name = ponding_area4 5 6 and unclogged
                if ((obj.name == "ponding area2" || obj.name == "ponding area3") && unclogged)
                { 
                    rend.material.SetVector("_Direction_1", new Vector2(0f, 1f)); 
                }

                obj.transform.position = new Vector3(obj.transform.position.x, -1.7f, obj.transform.position.z);
                float water_level = attributes[i].water_level;
                obj.transform.localScale = new Vector3(obj.transform.localScale.x, water_level, obj.transform.localScale.z);
                if (water_level <= 0f)
                {
                    obj.SetActive(false);
                }
                else
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    //Defines what happens when the main button (of the right controller) is trigger 
    protected override void TriggerMainButton()
    {
       
    }


    //Processes additional information contained in WorldJSONInfo - sent by GAMA at each simulation step.  
    protected override void ManageOtherInformation()
    {

    }



    public static event Action<GAMAMessage2> OnGAMAMessageReceived;

    protected override void ManageOtherMessages(string content)
    {
        message = GAMAMessage2.CreateFromJSON(content);
        OnGAMAMessageReceived?.Invoke(message);
    }

    //action activated at the end of the update phase (every frame)
    protected override void OtherUpdate()
    {

        //if (IsGameState(GameState.GAME) && UnityEngine.Random.Range(0.0f, 1.0f) < 0.002f)
        //{
        //    string mes = "A message from Unity at time: " + Time.time;
        //    //call the action "receive_message" from the unity_linker agent with two arguments: the id of the player and a message
        //    Dictionary<string, string> args = new Dictionary<string, string> {
        //         {"id",ConnectionManager.Instance.GetConnectionId() },
        //         {"mes",  mes }};

        //    //Debug.Log("sent to GAMA: " + mes);
        //    //ConnectionManager.Instance.SendExecutableAsk("receive_message", args);
        //}
        if (message != null)
        {
            if (!string.IsNullOrWhiteSpace(message.init_))
            {
                //Debug.Log(message.init_);
                GameObject pipes = GameObject.Find("PipeSystem");
                foreach (Transform obj in pipes.transform)
                {
                    //Debug.Log(obj.name);
                    ChangeColor(obj.gameObject, Color.blue);
                }
                if (message.init_ != "regular_state")
                {

                    GameObject blockedInletObj = GameObject.Find(message.init_);
                    //Debug.Log("looking for: " + message.init_ + " -> found: " + blockedInletObj);
                    if (blockedInletObj != null)
                        ChangeColor(blockedInletObj, Color.white);
                }
            }
            if (message.message_ != "")
            //Debug.Log("test");
            //StartCoroutine(ShowForDuration(message.cycle, 10f));
            {
                SendingMessages.Show(message.message_);
                //StartCoroutine(ShowForDuration(message.message_, 10f));
                Debug.Log("received from GAMA: " + message.message_);

            }
            if (!string.IsNullOrWhiteSpace(message.timer_start))
            {
                Debug.Log("timer reçu = " + message.timer_start);

                if (message.timer_start == "0")
                {
                    // 1. On efface le texte
                    timerText.text = "";

                    // 2. On coupe véritablement le timer s'il était en cours
                    if (timerCoroutine != null)
                    {
                        StopCoroutine(timerCoroutine);
                        //SendMessageToGama(progressBar.BarValue.ToString());
                        timerCoroutine = null; // On réinitialise la mémoire
                    }
                }
                else
                {
                    progressBar.BarValue = 0f;

                    // (Optionnel mais recommandé) Sécurité : on arrête un potentiel ancien timer
                    // avant d'en lancer un nouveau pour éviter qu'ils ne se superposent
                    if (timerCoroutine != null)
                    {
                        StopCoroutine(timerCoroutine);
                        //SendMessageToGama(progressBar.BarValue.ToString());
                    }

                    // On lance le nouveau timer et on le sauvegarde dans notre variable
                    timerCoroutine = StartCoroutine(CountDown(int.Parse(message.timer_start)));
                }
            }
            GameObject[] nbssAreaObjs = GameObject.FindGameObjectsWithTag("nbss_area");
            GameObject[] fmObjs = GameObject.FindGameObjectsWithTag("filter_media");
            GameObject[] parkObjs = GameObject.FindGameObjectsWithTag("park");
            GameObject lawnObj = GameObject.FindGameObjectWithTag("lawn");
            if (message.scenario == "1")
            {
                Debug.Log("Scenario1 enclenché");
                scenario = 1;
                Scenario1.SetActive(true);
                Scenario2.SetActive(false);

                dateTimeText.text = "12 avril\n11:37";

                // Assigning spring materials to vegetal components
                Material matNBSS = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Grass05");
                Material matLawn = Resources.Load<Material>("Materials/Lawn_Opaque");
                Material matPark = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Grass06");
                foreach (GameObject obj in nbssAreaObjs.Concat(fmObjs))
                {
                    obj.GetComponent<Renderer>().sharedMaterial = matNBSS;
                }
                lawnObj.GetComponent<Renderer>().sharedMaterial = matLawn;
                foreach (GameObject obj in parkObjs)
                {
                    obj.GetComponent<Renderer>().sharedMaterial = matPark;
                }

                // Update sky box to spring sky
                RenderSettings.skybox = defaultSky;
                DynamicGI.UpdateEnvironment();
            }
            if (message.scenario == "2")
            {

                Debug.Log("Scenario2 enclenché");
                scenario = 2;
                // changer matériau fm en sol sec
                // mettre soleil aveuglant + poussière
                // enable gameobject scenario 2
                Scenario1.SetActive(false);
                Scenario2.SetActive(true);

                dateTimeText.text = "3 août\n13:04";

                //reset paillage et elem changés par sc1
                GameObject[] paillageObjs = GameObject.FindGameObjectsWithTag("paillage");
                foreach (GameObject obj in paillageObjs)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                }

                // Assigning summer/dry materials to vegetal components
                Material matDrySoil = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Dry03");
                Material matSummerGrass0 = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Grass02");
                Material matSummerGrass1 = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Grass01");

                foreach (GameObject obj in nbssAreaObjs.Concat(fmObjs))
                {
                    obj.GetComponent<Renderer>().sharedMaterial = matDrySoil;
                }
                lawnObj.GetComponent<Renderer>().sharedMaterial = matSummerGrass0;
                foreach (GameObject obj in parkObjs)
                {
                    obj.GetComponent<Renderer>().sharedMaterial = matSummerGrass1;
                }

                // Update sky box to summer sky
                RenderSettings.skybox = summerSky;
                DynamicGI.UpdateEnvironment();
                

            }
            if (message.scenario == "2b" || message.scenario == "2c")
            {
                GameObject[] sprouts = GameObject.FindGameObjectsWithTag("shrubs_plants");
                if (sprouts.Length == 0)
                {
                    Debug.LogWarning("Aucun objet trouvé avec le tag 'shrubs_plants'.");
                    return;
                }
                GameObject shrubPrefab = Resources.Load<GameObject>("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Bush_1_1");
                foreach (GameObject sprout in sprouts)
                {
                    if (sprout == null) continue;
                    GameObject shrub = Instantiate(shrubPrefab, sprout.transform.position, sprout.transform.rotation);
                    if (sprout.transform.parent != null)
                    {
                        shrub.transform.SetParent(sprout.transform.parent);
                    }
                    //shrub.transform.localScale = sprout.transform.localScale;
                    shrub.tag = "shrubs_plants";
                    sprout.SetActive(false);
                }
                // grow trees
                GameObject[] treeSprouts = GameObject.FindGameObjectsWithTag("sprout");
                if (treeSprouts.Length == 0)
                {
                    Debug.LogWarning("Aucun objet trouvé avec le tag 'sprout'.");
                    return;
                }
                GameObject treePrefab = Resources.Load<GameObject>("Prefabs/Nature Biomes Pack - Low Poly/Prefabs/Tree 3 G1");
                foreach (GameObject sprout in treeSprouts)
                {
                    if (sprout == null) continue;
                    GameObject tree = Instantiate(treePrefab, sprout.transform.position, sprout.transform.rotation);
                    if (sprout.transform.parent != null)
                    {
                        tree.transform.SetParent(sprout.transform.parent);
                    }
                    //shrub.transform.localScale = sprout.transform.localScale;
                    tree.tag = "tree";
                    sprout.SetActive(false);
                }
                // grow local flora
                GameObject[] localFlora = GameObject.FindGameObjectsWithTag("local_flora");
                if (localFlora.Length == 0)
                {
                    Debug.LogWarning("Aucun objet trouvé avec le tag 'local_flora'.");
                    return;
                }
                GameObject grownFlora = Resources.Load<GameObject>("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Grass_1_2");
                foreach (GameObject obj in localFlora)
                {
                    if (obj == null) continue;
                    GameObject newFlora = Instantiate(grownFlora, obj.transform.position, obj.transform.rotation);
                    if (obj.transform.parent != null)
                    {
                        newFlora.transform.SetParent(obj.transform.parent);
                    }
                    //shrub.transform.localScale = sprout.transform.localScale;
                    newFlora.tag = "local_flora";
                    obj.SetActive(false);
                }
            }
            if (message.scenario == "2a" || message.scenario == "2b" || message.scenario == "2c")
            {
                dateTimeText.text = "23 septembre\n9h40";
                RenderSettings.skybox = defaultSky;
                DynamicGI.UpdateEnvironment();
            }
            if (message.scenario == "2a")
            {
                // fail
                // terre, toute la veg morte, arbres sans feuillage
                // changer la date
                GameObject treeNoLeaf = Resources.Load<GameObject>("Prefabs/Pine Forest Pack/Prefabs/PineNoLeaf");
                GameObject[] trees = GameObject.FindGameObjectsWithTag("tree");
                foreach (GameObject tree in trees)
                {
                    if (tree == null) continue;
                    GameObject newTree = Instantiate(treeNoLeaf, tree.transform.position, tree.transform.rotation);
                    if (tree.transform.parent != null)
                    {
                        newTree.transform.SetParent(tree.transform.parent);
                    }
                    //shrub.transform.localScale = sprout.transform.localScale;
                    newTree.tag = "tree";
                    tree.SetActive(false);
                }
                GameObject[] grass = GameObject.FindGameObjectsWithTag("grass");
                foreach (GameObject obj in grass)
                {
                    if (UnityEngine.Random.Range(0f, 100f) < 70f)
                    {
                        obj.gameObject.SetActive(false);
                    }
                    else continue;
                }
            }
            if (message.scenario == "2b")
            {
                // partial success
                // des endroits avec de la végétation verte, de l'eau dans les noues (polluées)
                Material greenMat = Resources.Load<Material>("Prefabs/Pine Forest Pack/Materials/PineForest");
                GameObject[] grass = GameObject.FindGameObjectsWithTag("grass");
                foreach (GameObject obj in grass)
                {
                    if (UnityEngine.Random.Range(0f, 100f) < 30f)
                    {
                        obj.transform.localScale = new Vector3(7f, 7f, 7f);
                        //enfant.gameObject.SetActive(false);
                    }
                    else if (UnityEngine.Random.Range(0f, 100f) < 50f)
                    {
                        Renderer rend = obj.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            //rend.material.color = new Color(166f / 255f, 153f / 255f, 34f / 255f);
                            rend.sharedMaterial = greenMat;
                        }
                    }
                    else continue;
                }
            }
            if (message.scenario == "2c")
            {
                // success
                // végétation verte, biodiv, bcp d'herbes

                // végétation verte
                Material matNBSS = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Grass05");
                Material matLawn = Resources.Load<Material>("Materials/Lawn_Opaque");
                Material matPark = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Grass06");
                foreach (GameObject obj in nbssAreaObjs.Concat(fmObjs))
                {
                    obj.GetComponent<Renderer>().sharedMaterial = matNBSS;
                }
                lawnObj.GetComponent<Renderer>().sharedMaterial = matLawn;
                foreach (GameObject obj in parkObjs)
                {
                    obj.GetComponent<Renderer>().sharedMaterial = matPark;
                }
                // herbe abondante et verte
                Material greenMat = Resources.Load<Material>("Prefabs/Pine Forest Pack/Materials/PineForest");
                GameObject[] grass = GameObject.FindGameObjectsWithTag("grass");
                foreach (GameObject obj in grass)
                {
                    obj.GetComponent<Renderer>().sharedMaterial = greenMat;
                    obj.transform.localScale = new Vector3(10f, 10f, 10f);
                }
                
            }
            if (message.scenario == "menu") {
                Debug.Log("Afficher menu");
                if (ResetScenarioObjects.Instance != null)
                {
                    Debug.Log("Instance trouvée, lancement du reset...");
                    ResetScenarioObjects.Instance.ResetToDefaultState();
                }
                else
                {
                    Debug.LogError("ERREUR : ResetScenarioObjects.Instance est NULL ! Le script n'est pas dans la scène ou n'a pas pu s'initialiser dans Awake.");
                }
                Scenario1.SetActive(false);
                Scenario2.SetActive(false);
                DisplayCanvas.SetActive(false);
                menuPanel.SetActive(true);
            }

            if (message.phase == "active")
            {
                interactionsAutorisees = true;
            }
            if (message.phase == "passive")
            {
                interactionsAutorisees = false;
                MenuRadialManager.Instance.FermerMenu();
            }
            message = null;
        }
    }
}


[System.Serializable]
public class GAMAMessage2
{
    public string message_;
    public string timer_start;
    public string init_;
    public string scenario;
    public string score;
    public string add_to_score;
    public string phase;

    public static GAMAMessage2 CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<GAMAMessage2>(jsonString);
    }
}