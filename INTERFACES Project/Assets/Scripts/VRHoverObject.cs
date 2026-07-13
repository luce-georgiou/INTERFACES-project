using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class VRHoverObjet : MonoBehaviour
{
    [Header("Configuration du texte")]
    public string messageAAfficher;

    [Header("Swale Health")]
    public float health = 0.5f; // à config avec attribut santé envoyé par gama pour chaque noue (dans intercation)

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
        // On appelle le Manager unique pour lui dire quoi afficher et où
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.AfficherTooltip(messageAAfficher, health, true, transform);
        }
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        // On dit au Manager de se cacher
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.MasquerTooltip();
        }
    }
}