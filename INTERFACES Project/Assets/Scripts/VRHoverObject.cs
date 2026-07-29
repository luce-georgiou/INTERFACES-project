using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class VRHoverObjet : MonoBehaviour
{
    [Header("Configuration du texte")]
    public string messageAAfficher;

    [Header("Swale Health")]
    public float health = 0.5f;

    [Header("Identifiant unique")]
    public int idGama;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
    }

    void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        int idx = int.Parse(gameObject.name.Replace("Empty", ""));
        string nameObject = "nbss_area" + idx;
        if (SimulationManager.Instance == null)
        {
            return;
        }
        float health = SimulationManager.Instance.GetHealth(nameObject);

        //Calling upon Tooltip manager to display message and health bar
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.AfficherTooltip(messageAAfficher, health, true, transform);
        }
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        // Disable display
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.MasquerTooltip();
        }
    }
}