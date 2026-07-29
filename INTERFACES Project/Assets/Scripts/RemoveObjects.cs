using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class RemoveObjects : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [SerializeField] private ProgressBar progressBarObj;
    public TMP_Text actionCountText;

    public string messageAfterSelect = "Les cendres acidifient le sol et tuent les micro-organismes.";
    public string messageHover = "";

    private int count;

    private Renderer objectRenderer;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        count = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Count(go => go.name.StartsWith("Cylinder"));

        objectRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(DesactiverObjet);

        // On écoute au Hover
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(DesactiverObjet);
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        // Si les interactions sont bloquées, on ne fait rien
        if (!SimulationManagerInteraction.interactionsAutorisees && SimulationManagerInteraction.scenario != 0) return;

        // On change la couleur en bleu
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = Color.blue;
        }
        SendingMessages.Show(messageHover);
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        // Le pointeur quitte l'objet, on remet la couleur d'origine
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = Color.white;
        }
    }

    private void DesactiverObjet(SelectEnterEventArgs args)
    {
        if (SimulationManagerInteraction.actionCount >= SimulationManagerInteraction.actionLimit) SimulationManagerInteraction.interactionsAutorisees = false;

        if (!SimulationManagerInteraction.interactionsAutorisees && SimulationManagerInteraction.scenario != 0) return;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        StartCoroutine(SequenceMessageEtDesactivation());
    }

    private IEnumerator SequenceMessageEtDesactivation()
    {

        if (SimulationManagerInteraction.scenario == 0)
        { 
            progressBarObj.BarValue = 100f;
            SimulationManager.Instance.SendMessageToGama("nbss_area0" + ":" + "20");
        }
        if (SimulationManagerInteraction.scenario == 2) progressBarObj.BarValue = progressBarObj.BarValue + 10f / count;

        Dictionary<string, string> args = new Dictionary<string, string> {
                 {"id",ConnectionManager.Instance.GetConnectionId() },
                 {"mes",  progressBarObj.BarValue.ToString() }};
        ConnectionManager.Instance.SendExecutableAsk("receive_message", args);

        SimulationManagerInteraction.actionCount += 1;
        actionCountText.text = "Actions restantes : " + SimulationManagerInteraction.actionCount + " / " + SimulationManagerInteraction.actionLimit;

        if (!string.IsNullOrEmpty(messageAfterSelect))
        {
            SendingMessages.Show(messageAfterSelect);
        }
        yield return null;

        Destroy(gameObject);
    }

}
