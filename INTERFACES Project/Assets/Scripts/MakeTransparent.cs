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

    private XRSimpleInteractable simpleInteractable;

    private MonoBehaviour xrTintScript;

    private bool isTransparent = false;

    void Awake()
    {
        transparentMat = Resources.Load<Material>("Materials/Lawn_Transparent");
        Color tempColor = transparentMat.color;
        tempColor.a = selectedAlpha;
        transparentMat.color = tempColor;
        opaqueMat = Resources.Load<Material>("Materials/Lawn_Opaque");
        
        originalColor = new Color(0f, 58f / 255f, 0f, 1f);
        rend = GetComponent<Renderer>();
        mat = rend.material;
        rend.material.color = opaqueMat.color;

        col = GetComponent<Collider>();
        simpleInteractable = GetComponent<XRSimpleInteractable>();

        // On cherche le script de Tint
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
        Debug.Log("1. Début de la séquence");
        isTransparent = true;
        yield return null;

        if (xrTintScript != null) xrTintScript.enabled = false;
        //SetAlpha(0.7f);
        //originalColor.a = selectedAlpha;
        //rend.material.color = originalColor;
        rend.material = transparentMat;

        Debug.Log("2. Alpha appliqué. Le collider va être désactivé.");
        col.enabled = false;

        Debug.Log("3. Collider désactivé ! Début du chrono de " + disableDuration + " secondes...");
        yield return new WaitForSeconds(disableDuration);

        Debug.Log("4. Les 30 secondes sont écoulées ! Réactivation du collider...");
        //originalColor.a = 1f;
        //rend.material.color = originalColor;
        rend.material = opaqueMat;
        //SetAlpha(1f);
        //Debug.Log("back to og color");
        col.enabled = true;

        if (xrTintScript != null) xrTintScript.enabled = true;
        isTransparent = false;

        Debug.Log("5. Tout est restauré avec succès.");
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
            simpleInteractable.selectEntered.RemoveListener(OnSelect);
    }
}