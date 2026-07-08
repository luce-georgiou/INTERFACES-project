using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class DisableObject : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [SerializeField] private ProgressBar progressBarObj;

    public int count;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        count = FindObjectsOfType<GameObject>()
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
        StartCoroutine(SequenceMessageEtDesactivation());
    }

    private IEnumerator SequenceMessageEtDesactivation()
    {
        //Collider col = GetComponent<Collider>();
        //if (col != null) col.enabled = false;

        //Renderer rend = GetComponent<Renderer>();
        //if (rend != null) rend.enabled = false;

        progressBarObj.BarValue = progressBarObj.BarValue + 10f / count;
        SendingMessages.Show("Les cendres acidifient le sol et tuent les micro-organismes.\n+10");
        yield return new WaitForSeconds(3f);
        SendingMessages.Show("");

        gameObject.SetActive(false);
    }
}
