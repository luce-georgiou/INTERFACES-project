using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class VRHoverObjetSimple : MonoBehaviour
{
    [Header("Configuration du texte")]
    public string messageAAfficher;

    private XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
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
        if (TooltipManager.Instance != null)
        {
            //Display text when controller is hovering over objects (no health bar)
            TooltipManager.Instance.AfficherTooltip(messageAAfficher, 0f, false, transform, new Vector3(0, -0.1f, 0), 3f);
        }
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.MasquerTooltip();
        }
    }
}
