using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MakeTransparent : MonoBehaviour
{
    [SerializeField] private float selectedAlpha;
    [SerializeField] private float disableDuration;

    private Collider col;
    private Color originalColor;
    public Renderer rend;
    public Material mat;

    public Material transparentMat;
    public Material opaqueMat;
    public Material summerGrassMat;

    private XRSimpleInteractable simpleInteractable;

    private MonoBehaviour xrTintScript;

    private bool isTransparent = false;

    void Awake()
    {
        transparentMat = Resources.Load<Material>("Materials/Lawn_Transparent"); //Transparent mat
        Color tempColor = transparentMat.color;
        tempColor.a = selectedAlpha;
        transparentMat.color = tempColor;
        opaqueMat = Resources.Load<Material>("Materials/Lawn_Opaque"); //Opaque mat for spring
        summerGrassMat = Resources.Load<Material>("YughuesFreeGroundMaterials/Materials/M_YFGM_Grass02"); //Opaque mat for summer

        originalColor = new Color(0f, 58f / 255f, 0f, 1f);
        rend = GetComponent<Renderer>();
        mat = rend.material;
        rend.material.color = opaqueMat.color;

        col = GetComponent<Collider>();
        simpleInteractable = GetComponent<XRSimpleInteractable>();

        xrTintScript = GetComponent("XRTintInteractableVisual") as MonoBehaviour;

        simpleInteractable.selectEntered.AddListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs args)
    {
        if (isTransparent) return;
        StartCoroutine(HandleTransparencyRoutine());
    }

    IEnumerator HandleTransparencyRoutine()
    {
        isTransparent = true;
        yield return null;

        if (xrTintScript != null) xrTintScript.enabled = false;
        rend.material = transparentMat;

        //Disable collider
        col.enabled = false;

        //Disable during 30s
        yield return new WaitForSeconds(disableDuration);

        if (SimulationManagerInteraction.scenario == 1)
        {
            rend.material = opaqueMat;
        }
        else if (SimulationManagerInteraction.scenario == 2)
        {
            rend.material = summerGrassMat;
        }
        
        col.enabled = true;

        if (xrTintScript != null) xrTintScript.enabled = true;
        isTransparent = false;
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
            simpleInteractable.selectEntered.RemoveListener(OnSelect);
    }
}