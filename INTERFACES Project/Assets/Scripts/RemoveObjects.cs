using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class RemoveObjects : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [SerializeField] private ProgressBar progressBarObj;

    public int count;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        count = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Count(go => go.name.StartsWith("Cylinder"));

        Debug.Log("Nombre de cylindres trouvés : " + count);
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(DesactiverObjet);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(DesactiverObjet);
    }

    private void DesactiverObjet(SelectEnterEventArgs args)
    {
        if (!SimulationManagerInteraction.interactionsAutorisees) return;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        StartCoroutine(SequenceMessageEtDesactivation());
    }

    private IEnumerator SequenceMessageEtDesactivation()
    {
        //Collider col = GetComponent<Collider>();
        //if (col != null) col.enabled = false;

        //Renderer rend = GetComponent<Renderer>();
        //if (rend != null) rend.enabled = false;

        progressBarObj.BarValue = progressBarObj.BarValue + 10f / count;

        Dictionary<string, string> args = new Dictionary<string, string> {
                 {"id",ConnectionManager.Instance.GetConnectionId() },
                 {"mes",  progressBarObj.BarValue.ToString() }};
        ConnectionManager.Instance.SendExecutableAsk("receive_message", args);

        SendingMessages.Show("Les cendres acidifient le sol et tuent les micro-organismes.\n+10");
        yield return new WaitForSeconds(3f);
        //SendingMessages.Show("");

        Destroy(gameObject);
        //gameObject.SetActive(false);
    }

}
