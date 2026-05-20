using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit; 
using UnityEngine.InputSystem;
using System.Linq;

using DigitalRuby.RainMaker;



public class SimulationManagerInteraction : SimulationManager
{

    //Defines what happens when a ray passes over an object 
    protected override void HoverEnterInteraction(HoverEnterEventArgs ev)
    {
         GameObject obj = ev.interactableObject.transform.gameObject;
        //if (obj.tag.Equals("inlet") || obj.tag.Equals("outlet"))
        //{
        //    ChangeColor(obj, Color.blue);
        //}
        Debug.Log("HoverEnterInteraction : " + obj);
        if (obj.tag.Equals("inlet") || obj.tag.Equals("weeds"))
        {
            Debug.Log("HoverEnterInteraction : " + obj);
            SimulationManagerSolo.ChangeColor(obj, Color.blue);
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
        Debug.Log("HoverExitInteraction : " + obj);
        if (obj.tag.Equals("inlet") || obj.tag.Equals("weeds"))
        {
            SimulationManagerSolo.ChangeColor(obj, Color.white);
        }
    }

    //Defines what happens when a object is selected
    protected override void SelectInteraction(SelectEnterEventArgs ev)
    {

        if (remainingTime <= 0.0)
        {
            GameObject grabbedObject = ev.interactableObject.transform.gameObject;
            Debug.Log("grabbedObject : " + grabbedObject);

            if (grabbedObject.tag.Equals("inlet") || grabbedObject.tag.Equals("weeds"))
            {
                Dictionary<string, string> args = new Dictionary<string, string> {
                         {"id", grabbedObject.name }
                    };
                ConnectionManager.Instance.SendExecutableAsk("maintenance_remove", args);

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


        }

    }

    private List<string> tagsToIgnore = new List<string> { "ponding_area", "swale", "filter_media", "gravel" };//, "grass", "trees", "trash", "weeds", "shrubs_plants", "vegetal_waste"};

    protected override void ManageAttributes(List<Attributes> attributes)
    {
        
        for (int i = 0; i < infoWorld.names.Count; i++)
        {
            string name = infoWorld.names[i];

            if (tagsToIgnore.Any(tag => name.StartsWith(tag)))
                continue;

            Debug.Log(name);
            int type = attributes[i].fqt_inlet;
            //int fqt_outlet = attributes[i].fqt_outlet;

            //if (name.StartsWith("rain"))
            //{
            //    GameObject rainObj = GameObject.Find("Rain");
            //    BaseRainScript rain = rainObj.GetComponent<BaseRainScript>();
            //    rain.RainIntensity = type / 3f; // mappe 0-3 vers 0.0-1.0
            //}

            List<object> o = geometryMap[name];
            GameObject obj = (GameObject)o[0];

            if (name.StartsWith("inlet"))
            {
                int fqt = attributes[i].fqt_inlet;
                Debug.Log("inlet fqt : " + fqt);
                
                if (fqt == 0)      ChangeColor(obj, Color.red);
                else if (fqt == 1) ChangeColor(obj, Color.orange);
                else if (fqt == 2) ChangeColor(obj, Color.yellow);
                else if (fqt == 3) Debug.Log("test send dyn data");
            }
            else if (name.StartsWith("outlet"))
            {
                int fqt = attributes[i].fqt_outlet;
                Debug.Log("outlet fqt : " + fqt);

                if (fqt == 0)      ChangeColor(obj, Color.red);
                else if (fqt == 1) ChangeColor(obj, Color.orange);
                else if (fqt == 2) ChangeColor(obj, Color.yellow);
                else if (fqt == 3) Debug.Log("test send dyn data");
                
            }
            else if (name.StartsWith("rain"))
            {
                // Change rain intensity according to rain data
                float intensity = attributes[i].rain_intensity;
                GameObject rainObj = GameObject.FindWithTag("rain");

                //if (rainObj == null)
                //{
                //    BaseRainScript rain = FindObjectOfType<BaseRainScript>();
                //    if (rain != null)
                //    {
                //        rain.RainIntensity = intensity / 3f;
                //    }
                //    else
                //    {
                //        Debug.LogError("BaseRainScript introuvable");
                //    }
                //    continue;
                //}

                BaseRainScript rainScript = rainObj.GetComponent<BaseRainScript>();
                rainScript.RainIntensity = intensity / 3f; // mappe 0-3 vers 0.0-1.0

                // Change ponding area aspect according to rain intensity
                string pondingArea = "ponding_area0";
                if (geometryMap.ContainsKey(pondingArea)) {
                    GameObject pondObj = (GameObject)geometryMap[pondingArea][0];
                    if (intensity == 0) continue;
                    else if (intensity == 1) ChangeColor(pondObj, Color.yellow);
                    else if (intensity == 2) ChangeColor(pondObj, Color.orange);
                    else if (intensity == 3) ChangeColor(pondObj, Color.red);
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
            // gestion de l'aspect de l'environnement selon les saisons
            else if (name.StartsWith("trees"))
            {
                string saison = attributes[i].tree_seasons;
                GameObject sapinPrefab = Resources.Load<GameObject>("Prefabs/Snowy_Low_Poly_Trees/Pine_Snowy1");
                GameObject treePrefab = Resources.Load<GameObject>("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Tree_1_1");
                GameObject treeObj = GameObject.FindWithTag("trees");
                if (saison == "winter") // mettre neige à la place pluie, Ground blanc
                {
                    Vector3 position = treeObj.transform.position;
                    Quaternion rotation = treeObj.transform.rotation;

                    Destroy(treeObj);
                    Instantiate(sapinPrefab, position, rotation);


                    //obj.GetComponent<MeshFilter>().mesh = sapinPrefab.GetComponent<MeshFilter>().sharedMesh;
                    //obj.GetComponent<MeshRenderer>().material = sapinPrefab.GetComponent<MeshRenderer>().sharedMaterial;
                }
                else if (saison == "spring")
                {
                    // ajouter fleurs, sol vert, arbre classique
                }
                else if (saison == "fall")
                {
                    // arbre sans feuille, sol orange, bcp de vegetal_waste (changer aspect vegetal_waste selon saison?)
                }
                else if (saison == "summer")
                {
                    // à décider, sécheresse
                }
            }
        }
    }
    

    //Defines what happens when the main button (of the right controller) is trigger 
    protected override void TriggerMainButton()
    {
       
    }

    //Defines what happens when a non-standard message is received from GAMA. 
    protected override void ManageOtherMessages(string content)
    {

    }

    //Processes additional information contained in WorldJSONInfo - sent by GAMA at each simulation step.  
    protected override void ManageOtherInformation()
    {

    }


    //Adds extra actions to be performed for each new frame.
    protected override void OtherUpdate()
    {

    }

   
}