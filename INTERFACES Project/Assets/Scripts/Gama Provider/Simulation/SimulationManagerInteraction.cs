using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit; 
using UnityEngine.InputSystem;
using System.Linq;



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

    private List<string> tagsToIgnore = new List<string> { "ponding_area"};

    protected override void ManageAttributes(List<Attributes> attributes)
    {
        
        for (int i = 0; i < infoWorld.names.Count; i++)
        {
            string name = infoWorld.names[i];

            if (tagsToIgnore.Any(tag => name.StartsWith(tag)))
                continue;

            Debug.Log(name);
            int type = attributes[i].type;
            Debug.Log("Health of inlet : " + type);
            List<object> o = geometryMap[name];
            GameObject obj = (GameObject)o[0];
            if (type == 0)
            {
                ChangeColor(obj, Color.red);
            }
            else if (type == 1)
            {
                ChangeColor(obj, Color.orange);
            }
            else if (type == 2)
            {
                ChangeColor(obj, Color.yellow);
            }
            else if (type == 3)
            {
                Debug.Log("test send dyn data");
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