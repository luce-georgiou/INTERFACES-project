using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("Attention : Aucune caméra avec le tag 'MainCamera' n'a été trouvée dans la scène !");
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Fait pivoter le Canvas pour qu'il soit parfaitement parallèle à l'écran du casque.
        // On additionne la position du Canvas et la direction de la caméra pour éviter que le texte soit inversé (effet miroir).
        transform.LookAt(transform.position + cameraTransform.forward);
    }
}
