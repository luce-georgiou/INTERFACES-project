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

    public TMP_Text timerText;
    public ProgressBar progressBar;
    public GameObject exclamationCanvas;
    private List<Attributes> lastAttributes;

    // Mettre dans cette liste tous les objets statiques
    private List<string> tagsToIgnore = new List<string> { "swale", "gravel", "grass", "flower", "NBSS", "road", "building", "park" };//, "grass", "trees", "trash", "weeds", "shrubs_plants", "vegetal_waste"};

    // Message manager
    //private string lastFailureMessage = "";
    IEnumerator ShowForDuration(string msg, float duration)
    {
        SendingMessages.Show(msg);
        yield return new WaitForSeconds(duration);
        SendingMessages.Show("");
    }

    
    private bool timerRunning = false;
    IEnumerator CountDown(int duration)
    {
        Debug.Log("CountDown started: " + duration);
        if (timerText == null) { Debug.LogError("timerText is null!"); yield break; }
        if (timerRunning) yield break;
        timerRunning = true;
        int remaining = duration;
        while (remaining > 0)
        {
            timerText.text = remaining / 60 + ":" + (remaining % 60 < 10 ? "0" : "") + remaining % 60;
            yield return new WaitForSeconds(1f);
            remaining--;
        }
        timerText.text = "0:00";
        timerRunning = false;
    }

    

    //Defines what happens when a ray passes over an object 
    protected override void HoverEnterInteraction(HoverEnterEventArgs ev)
    {
         GameObject obj = ev.interactableObject.transform.gameObject;
        //if (obj.tag.Equals("inlet") || obj.tag.Equals("outlet"))
        //{
        //    ChangeColor(obj, Color.blue);
        //}
        //Debug.Log("HoverEnterInteraction : " + obj);
        if (obj.tag.Equals("trash") || obj.tag.Equals("weeds"))
        {
            //Debug.Log("HoverEnterInteraction : " + obj);
            SimulationManagerSolo.ChangeColor(obj, Color.blue);
            SendingMessages.Show("Nettoyer ?");
        }
        if (obj.tag.Equals("filter_media"))
        {
            SimulationManagerSolo.ChangeColor(obj, Color.blue);
            SendingMessages.Show("Effectuer curage ?");
        }
    }


    //Defines what happens when a ray passes not anymore over an object 
    protected override void HoverExitInteraction(HoverExitEventArgs ev)
    {
        GameObject obj = ev.interactableObject.transform.gameObject;
        //if (obj.tag.Equals("inlet") || obj.tag.Equals("outlet"))
        //{
        //    bool isSelected = SelectedObjects.Contains(obj);
        //    ChangeColor(obj, isSelected ? Color.red : Color.gray);
        //}
        //Debug.Log("HoverExitInteraction : " + obj);
        if (obj.tag.Equals("trash") || obj.tag.Equals("weeds") || obj.tag.Equals("filter_media"))
        {
            SimulationManagerSolo.ChangeColor(obj, Color.white);
            SendingMessages.Show("");
        }
    }

    //Defines what happens when a object is selected
    protected override void SelectInteraction(SelectEnterEventArgs ev)
    {

        if (remainingTime <= 0.0)
        {
            GameObject grabbedObject = ev.interactableObject.transform.gameObject;
            //Debug.Log("grabbedObject : " + grabbedObject);
            int count;
            float weight = 20f;
            if (grabbedObject.tag.Equals("weeds") || grabbedObject.tag.Equals("trash"))
            {
                Dictionary<string, string> args = new Dictionary<string, string> {
                         {"id", grabbedObject.name }
                    };
                count = GameObject.FindGameObjectsWithTag(grabbedObject.tag).Length;
                ConnectionManager.Instance.SendExecutableAsk("maintenance_remove", args);
                progressBar.BarValue = progressBar.BarValue + (weight / count);
            }
            //GameObject obj = ev.interactableObject.transform.gameObject;
            //if (obj.tag.Equals("road"))
            //{
            //    Dictionary<string, string> args = new Dictionary<string, string>
            //    {
            //        {"id", obj.name }
            //    };
            //    ConnectionManager.Instance.SendExecutableAsk("block_road", args);
            //    bool newSelection = !SelectedObjects.Contains(obj);
            //    if (newSelection) SelectedObjects.Add(obj);
            //    else SelectedObjects.Remove(obj);

            //    ChangeColor(obj, newSelection ? Color.red : Color.gray);
            //    remainingTime = timeWithoutInteraction;
            //}
            else if (grabbedObject.tag.Equals("lawn_mower"))
            {
                Dictionary<string, string> args = new Dictionary<string, string> {
                         {"id", grabbedObject.name }
                    };
                ConnectionManager.Instance.SendExecutableAsk("mow_lawn", args);

            }
            else if (grabbedObject.tag.Equals("filter_media")) {
                Dictionary<string, string> args = new Dictionary<string, string> {
                         {"id", grabbedObject.name }
                    };
                ConnectionManager.Instance.SendExecutableAsk("curage", args);

                //if (infoWorld == null || lastAttributes == null) return;
                //int idx = infoWorld.names.IndexOf(grabbedObject.name);
                //Debug.Log(idx);
                //int fqt;
                //if (idx >= 0)
                //{
                //    fqt = lastAttributes[idx].fqt_fm; 
                //    if (fqt >= 2)
                //    {
                //        Debug.LogError("noue en bon état");
                //    }
                //    else
                //    {
                        progressBar.BarValue = progressBar.BarValue + 30f;
                        exclamationCanvas.SetActive(false);
                        Debug.LogError("curage done");
                //    }
                //}
                
                    
            }

        }

    }

    // To find vertices of objects
    Vector3[] GetNBSSVertices(string nbssName)
    {
        GameObject[] nbssObjects = GameObject.FindGameObjectsWithTag("park");
        foreach (GameObject obj in nbssObjects)
        {
            if (obj.name == nbssName)
            {
                Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
                if (mesh == null) return null;

                Vector3[] worldVertices = new Vector3[mesh.vertices.Length];
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    worldVertices[i] = obj.transform.TransformPoint(mesh.vertices[i]);
                }
                return worldVertices;
            }
        }
        return null;
    }

    
    
    // Liste toIgnore pour cette méthode
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

            string[] prefixes = { "inlet", "outlet" };
            foreach (string prefix in prefixes)
            {
                if (name.StartsWith(prefix))
                {
                    int fqt = prefix == "inlet" ? attributes[i].fqt_inlet : attributes[i].fqt_outlet;

                    if (fqt == 0)
                    {
                        ChangeColor(obj, Color.red);
                    }
                    else if (fqt == 1)
                    {
                        ChangeColor(obj, Color.orange);
                    }
                    else if (fqt == 2)
                    {
                        ChangeColor(obj, Color.yellow);
                    }
                    /* Show failure name ingame */
                    //string failure_name = prefix == "inlet" ? attributes[i].failures_inlet : attributes[i].failures_outlet;
                    //string newMessage = failure_name + " on " + name;
                    //if ((newMessage != lastFailureMessage) && (failure_name != null))
                    //{
                    //    lastFailureMessage = newMessage;
                    //    StartCoroutine(ShowForDuration(newMessage, 2f));
                    //}
                    //break;
                }
            }
            //if (name.StartsWith("inlet"))
            //{
            //    int fqt = attributes[i].fqt_inlet;
            //    //Debug.Log("inlet fqt : " + fqt);

            //    if (fqt == 0) ChangeColor(obj, Color.red);
            //    else if (fqt == 1) ChangeColor(obj, Color.orange);
            //    else if (fqt == 2) ChangeColor(obj, Color.yellow);
            //    //else if (fqt == 3) ; //Debug.Log("test send dyn data");

            //    string failure_name = attributes[i].failures_inlet;
            //    string newMessage = failure_name + " on " + name;        
            //    if ((newMessage != lastFailureMessage) && (failure_name != null))
            //    {
            //        lastFailureMessage = newMessage;
            //        StartCoroutine(ShowForDuration(newMessage, 2f));
            //    }
            //}
            //else if (name.StartsWith("outlet"))
            //{
            //    int fqt = attributes[i].fqt_outlet;
            //    //Debug.Log("outlet fqt : " + fqt);

            //    if (fqt == 0)      ChangeColor(obj, Color.red);
            //    else if (fqt == 1) ChangeColor(obj, Color.orange);
            //    else if (fqt == 2) ChangeColor(obj, Color.yellow);
            //    //else if (fqt == 3) Debug.Log("test send dyn data");

            //}
            //else if (name.StartsWith("failure_event"))
            //{
            //    GameObject failObj = GameObject.FindWithTag("failure_event");
            //    failObj.GetComponent<Renderer>().enabled = false;

            //    string failure_name = attributes[i].failure_name;
            //    string impacted_component = attributes[i].impacted_component;
            //    Debug.Log(failure_name + " on " + impacted_component);

            //    SendingMessages.Show(failure_name + " on " + impacted_component); 
            //}

            

            if (name.StartsWith("rain"))
            {
                float intensity = attributes[i].rain_intensity;
                string season = attributes[i].rain_seasons;
                BaseRainScript rainScript = GameObject.FindWithTag("rain").GetComponent<BaseRainScript>();
                ParticleSystem ps = rainScript.RainFallParticleSystem;

                var ma = ps.main;

                rainScript.RainIntensity = intensity / 3f;
                if (season == "winter") // hiver
                {
                    ma.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.25f);
                    ma.gravityModifier = 0.2f; // presque pas de gravit�
                    //ma.maxParticles = 6000;
                    rainScript.EnableWind = false;
                    var renderer = rainScript.RainFallParticleSystem.GetComponent<ParticleSystemRenderer>();
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;

                    var vol = rainScript.RainFallParticleSystem.velocityOverLifetime;
                    vol.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f); // quasi rien
                    vol.y = new ParticleSystem.MinMaxCurve(-0.5f, -0.1f);  // descente
                    vol.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f); // quasi rien
                }
                else
                {
                    ma.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
                    ma.gravityModifier = 1f;
                    rainScript.EnableWind = true;
                    var renderer = rainScript.RainFallParticleSystem.GetComponent<ParticleSystemRenderer>();
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    var vol = rainScript.RainFallParticleSystem.velocityOverLifetime;
                    vol.x = new ParticleSystem.MinMaxCurve(0f, 0f);
                    vol.y = new ParticleSystem.MinMaxCurve(-55f, -50f);
                    vol.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                }


                //// Change rain intensity according to rain data -> marche
                //float intensity = attributes[i].rain_intensity;
                //int saison = attributes[i].rain_seasons;
                //GameObject rainObj = GameObject.FindWithTag("rain");
                //BaseRainScript rainScript = rainObj.GetComponent<BaseRainScript>();
                //rainScript.RainIntensity = intensity / 3f; // mappe 0-3 vers 0.0-1.0

                // Change ponding area aspect according to rain intensity
                foreach (var key in geometryMap.Keys)
                {
                    if (!key.StartsWith("ponding_area")) continue;

                    GameObject pondObj = (GameObject)geometryMap[key][0];

                    //Material mat_dry = pondObj.GetComponent<Renderer>().material;
                    ////pondObj.transform.localScale = new Vector3(pondObj.transform.localScale.x, 2f, pondObj.transform.localScale.z);
                    //Material mat_wet = Resources.Load<Material>("Simple Water Shader/Resources/Water_mat_01");
                    //Debug.Log("Material: " + mat_wet);
                    //mat_wet.SetFloat("_Depth", 1f);
                    //pondObj.GetComponent<Renderer>().material = mat_wet;
                    //Debug.Log("Material assigned: " + pondObj.GetComponent<Renderer>().material.name);
                    ////pondObj.GetComponent<Renderer>().material = intensity != 0 ? mat_wet : mat_dry; ;


                    //if (intensity == 0)
                    //{
                    //    ChangeColor(pondObj, new Color(0.6f, 0.7f, 0.3f));
                    //    pondObj.transform.localScale = new Vector3(pondObj.transform.localScale.x, 0.2f, pondObj.transform.localScale.z);
                    //}
                    //else if (intensity == 1)
                    //{
                    //    ChangeColor(pondObj, Color.blue);
                    //    pondObj.transform.localScale = new Vector3(pondObj.transform.localScale.x, 0.3f, pondObj.transform.localScale.z);
                    //}
                    //else if (intensity == 2)
                    //{
                    //    ChangeColor(pondObj, Color.blue);
                    //    pondObj.transform.localScale = new Vector3(pondObj.transform.localScale.x, 0.5f, pondObj.transform.localScale.z);
                    //}
                    //else if (intensity == 3)
                    //{
                    //    ChangeColor(pondObj, Color.blue);
                    //    pondObj.transform.localScale = new Vector3(pondObj.transform.localScale.x, 0.8f, pondObj.transform.localScale.z);
                    //}

                }
                //foreach (var key in geometryMap.Keys.Where(k => k.StartsWith("grass")))
                //{
                //    GameObject herbeObj = (GameObject)geometryMap[key][0];
                //    if (intensity == 0) ChangeColor(herbeObj, Color.yellow);
                //    else if (intensity == 1) ChangeColor(herbeObj, new Color(0.5f, 0.8f, 0.2f));
                //    else if (intensity == 2) ChangeColor(herbeObj, Color.green);
                //    else if (intensity == 3) ChangeColor(herbeObj, new Color(0f, 0.5f, 0f));
                //}
                //GameObject groundObj = GameObject.Find("Ground"); si objet dans hierarchy
            }
            else if (name.StartsWith("filter_media"))
            {
                obj.transform.position = new Vector3(obj.transform.position.x, -3f, obj.transform.position.z);
                Material mat = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Ground02");
                Color c = mat.color;
                if (name == "filter_media5")
                {

                    mat.SetFloat("_Surface", 1f); // Transparent
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = 3000;

                    c.a = 0.6f;
                    mat.SetColor("_BaseColor", c);
                }
                else
                {
                    c.a = 1f;
                    mat.SetColor("_BaseColor", c);
                }
                obj.GetComponent<Renderer>().material = mat;
                
                
                
                int fqt = attributes[i].fqt_fm;
                if (fqt <= 1)
                {
                    int idx = int.Parse(obj.name.Replace("filter_media", ""));
                    GameObject stairsObj0 = GameObject.Find("stairs" + idx + "_0");
                    GameObject stairsObj1 = GameObject.Find("stairs" + idx + "_1");
                    ChangeColor(obj, Color.red);
                    ChangeColor(stairsObj0, Color.red);
                    ChangeColor(stairsObj1, Color.red);
                    exclamationCanvas.SetActive(true);
                    exclamationCanvas.transform.localScale = new Vector3(0.1f, 0.1f, 100f);
                    exclamationCanvas.transform.position = obj.transform.position + Vector3.up * 8f;
                }
                else
                {
                    int idx = int.Parse(obj.name.Replace("filter_media", ""));
                    GameObject stairsObj0 = GameObject.Find("stairs" + idx + "_0");
                    GameObject stairsObj1 = GameObject.Find("stairs" + idx + "_1");
                    ChangeColor(obj, c);
                    ChangeColor(stairsObj0, c);
                    ChangeColor(stairsObj1, c);
                    //exclamationCanvas.SetActive(false);
                }
            }
            // gestion de l'aspect de l'environnement selon les saisons
            else if (name.StartsWith("trees"))
            {

                //string saison = attributes[i].tree_seasons;
                //GameObject sapinPrefab = Resources.Load<GameObject>("Prefabs/Snowy_Low_Poly_Trees/Pine_Snowy1");
                //GameObject treePrefab = Resources.Load<GameObject>("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Tree_1_1");
                ////GameObject treeObj = GameObject.FindWithTag("trees");


                //if (saison == "winter") // mettre neige � la place pluie, Ground blanc
                //{

                //    // Changer arbre en arbre enneig� 
                //    GameObject treeObj = GameObject.FindWithTag("trees");
                //    Vector3 position = treeObj.transform.position;
                //    Quaternion rotation = treeObj.transform.rotation;

                //    Destroy(treeObj);
                //    GameObject sapin = Instantiate(sapinPrefab, position, rotation);
                //    sapin.tag = "trees";


                //    //obj.GetComponent<MeshFilter>().mesh = sapinPrefab.GetComponent<MeshFilter>().sharedMesh;
                //    //obj.GetComponent<MeshRenderer>().material = sapinPrefab.GetComponent<MeshRenderer>().sharedMaterial;
                //}
                //else if (saison == "spring")
                //{
                //    GameObject sapinObj = GameObject.FindWithTag("trees");
                //    if (sapinObj != null)
                //    {
                //        Vector3 position = sapinObj.transform.position;
                //        Quaternion rotation = sapinObj.transform.rotation;
                //        Destroy(sapinObj);
                //        GameObject tree = Instantiate(treePrefab, position, rotation);
                //        tree.tag = "trees";
                //    }
                //    // ajouter fleurs, sol vert, arbre classique
                //}
                //else if (saison == "fall")
                //{
                //    GameObject sapinObj = GameObject.FindWithTag("trees");
                //    if (sapinObj != null)
                //    {
                //        Vector3 position = sapinObj.transform.position;
                //        Quaternion rotation = sapinObj.transform.rotation;
                //        Destroy(sapinObj);
                //        GameObject tree = Instantiate(treePrefab, position, rotation);
                //        tree.tag = "trees";
                //    }
                //    // arbre sans feuille, sol orange, bcp de vegetal_waste (changer aspect vegetal_waste selon saison?)
                //}
                //else if (saison == "summer")
                //{
                //    GameObject sapinObj = GameObject.FindWithTag("trees");
                //    if (sapinObj != null)
                //    {
                //        Vector3 position = sapinObj.transform.position;
                //        Quaternion rotation = sapinObj.transform.rotation;
                //        Destroy(sapinObj);
                //        GameObject tree = Instantiate(treePrefab, position, rotation);
                //        tree.tag = "trees";
                //    }
                //    // � d�cider, s�cheresse
                //}
                string saison = attributes[i].tree_seasons;
                //GameObject treeObj = GameObject.FindWithTag("trees");
                //List<object> o = geometryMap[name];
                //GameObject obj = (GameObject)o[0];

                // marche avec �a
                //List<object> treeData = geometryMap[name];
                //GameObject treeObj = (GameObject)treeData[0];

                GameObject lawn = GameObject.FindWithTag("lawn");
                if (saison == "winter")
                {
                    GameObject sapinPrefab = Resources.Load<GameObject>("Prefabs/Snowy_Low_Poly_Trees/Pine_Snowy1");


                    ChangeColor(lawn, Color.white); // snow in winter

                    if (sapinPrefab != null)
                    {
                        GameObject snow = Instantiate(sapinPrefab, obj.transform.position, obj.transform.rotation);
                        snow.name = "SnowLayer";
                        snow.transform.SetParent(obj.transform);
                    }
                }
                else
                {
                    ChangeColor(lawn, new Color(0f, 53f / 255f, 0f));
                    Transform snowLayer = obj.transform.Find("SnowLayer");
                    if (snowLayer != null)
                    {
                        Destroy(snowLayer.gameObject);
                    }
                }
            }
            else if (name.StartsWith("weeds") || name.StartsWith("trash"))
            {
                obj.transform.position = new Vector3(obj.transform.position.x, -1.6f, obj.transform.position.z);
            }
            else if (name.StartsWith("ponding_area"))
            {
                Material mat = Resources.Load<Material>("Materials/Water2/WaterVoronoi");
                obj.GetComponent<Renderer>().material = mat;
                obj.transform.position = new Vector3(obj.transform.position.x, -1.6f, obj.transform.position.z);

                //Debug.Log("test");
                //GameObject pondObj = GameObject.FindWithTag("pond");
                //Debug.Log("pond: " + pondObj);
                float water_level = attributes[i].water_level;
                //Debug.Log("water: " + water_level);
                //if (name == "ponding_area5")
                //{
                obj.transform.localScale = new Vector3(obj.transform.localScale.x, water_level, obj.transform.localScale.z);
                //Debug.Log(name + " water_level: " + water_level);
            }
            //else if (name.StartsWith("lawn"))
            //{
            //    float height = attributes[i].lawn_height;
            //    string season = attributes[i].lawn_seasons;
            //    obj.transform.localScale = new Vector3(obj.transform.localScale.x, height, obj.transform.localScale.z);
            //    if (season == "winter")
            //    {
            //        ChangeColor(obj, Color.white); // snow in winter
            //    }
            //    else
            //    {
            //        ChangeColor(obj, new Color(0f, 0.502f, 0f));
            //    }
            //}
        }
    }

    //SendingMessages message = null;

    ////allow to serialize the message as GAMAMessage object
    //protected override void ManageOtherMessages(string content)
    //{
    //    Debug.Log("receive message");
    //    message = SendingMessages.CreateFromJSON(content);
    //}

    ////action activated at the end of the update phase (every frame)
    //protected override void OtherUpdate()
    //{
    //    // if a message was received, display in the console the content of the message
    //    if (message != null)
    //    {
    //        Debug.Log("Test");
    //        Debug.Log("received from GAMA: " + message.name_failure_event + " on " + message.component);
    //        message = null;
    //    }
    //}

    //public TextMeshProUGUI messageText;

    //void Start()
    //{
    //    messageText.text = "Votre message";
    //}

    //public void ShowMessage(string msg)
    //{
    //    messageText.text = msg;
    //}
    private int swaleCount = 0;

    protected ProBuilderMesh BuildSwale(float width, float height, float depth, int stepCount, Vector3 location)
    {
        ProBuilderMesh stairs = ShapeGenerator.GenerateStair(PivotLocation.Center,
            new Vector3(width, height, depth),
            stepCount, false);
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.green; // ta couleur
        stairs.GetComponent<MeshRenderer>().material = mat;
        stairs.transform.position = location;
        stairs.gameObject.name = "swale__" + swaleCount;
        swaleCount++;
        stairs.gameObject.tag = "swale";
        stairs.ToMesh();
        stairs.Refresh();
        return stairs;
    }

    protected void BuildEnvironment()
    {
        // Swale 4
        BuildSwale(24.5f, 1.5f, 2f, 5, new Vector3(90f, 0.0f, 45.64999f)).transform.rotation = Quaternion.Euler(0, -90, 0);
        BuildSwale(24.5f, 1.5f, 2f, 5, new Vector3(93f, 0.0f, 45.64999f)).transform.rotation = Quaternion.Euler(0, 90, 0); // tourne de 90�;
    }


    //void Start()
    //{
    //    BuildEnvironment();
    //}

    //Defines what happens when the main button (of the right controller) is trigger 
    protected override void TriggerMainButton()
    {
       
    }


    //Processes additional information contained in WorldJSONInfo - sent by GAMA at each simulation step.  
    protected override void ManageOtherInformation()
    {

    }



    GAMAMessage2 message = null;

    protected override void ManageOtherMessages(string content)
    {
        message = GAMAMessage2.CreateFromJSON(content);
    }

    //action activated at the end of the update phase (every frame)
    protected override void OtherUpdate()
    {

        if (IsGameState(GameState.GAME) && UnityEngine.Random.Range(0.0f, 1.0f) < 0.002f)
        {
            string mes = "A message from Unity at time: " + Time.time;
            //call the action "receive_message" from the unity_linker agent with two arguments: the id of the player and a message
            Dictionary<string, string> args = new Dictionary<string, string> {
                 {"id",ConnectionManager.Instance.GetConnectionId() },
                 {"mes",  mes }};

            //Debug.Log("sent to GAMA: " + mes);
            //ConnectionManager.Instance.SendExecutableAsk("receive_message", args);
        }
        //timerText.text = "";
        if (message != null)
        {
            if (message.message_ != "")
            //Debug.Log("test");
            //StartCoroutine(ShowForDuration(message.cycle, 10f));
            {
                //messageQueue.Enqueue(message.message_);
                StartCoroutine(ShowForDuration(message.message_, 10f));
                Debug.Log("received from GAMA: " + message.message_);
                
            }
            if (!string.IsNullOrWhiteSpace(message.timer_start))
            {
                Debug.Log("timer reçu = " + message.timer_start);
                if (message.timer_start == "0")
                    timerText.text = "";
                else
                    progressBar.BarValue = 0f;
                    StartCoroutine(CountDown(int.Parse(message.timer_start)));
            }
            message = null;
        }


    }

    // à modifier pour chaque sortie d'eau vers nappe phréatique
    //public GameObject Arrow;

    //void Start()
    //{
    //    CreateArrow(new Vector3(0, 0, 0), new Vector3(5, 0, 0));
    //    CreateArrow(new Vector3(10, 0, 0), new Vector3(15, 0, 5));
    //    CreateArrow(new Vector3(20, 0, 0), new Vector3(25, 0, -5));
    //}

    //void CreateArrow(Vector3 start, Vector3 end)
    //{
    //    GameObject arrow = Instantiate(Arrow);
    //    FlowArrow fa = arrow.GetComponent<FlowArrow>();
    //    fa.startPoint = start;
    //    fa.endPoint = end;
    //}
}


[System.Serializable]
public class GAMAMessage2
{


    //public int cycle;
    public string message_;
    public string timer_start;

    public static GAMAMessage2 CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<GAMAMessage2>(jsonString);
    }


}