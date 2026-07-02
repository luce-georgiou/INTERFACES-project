using UnityEngine;
using TMPro; // Pour le TextMeshPro

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI")]
    public TextMeshProUGUI texteAffichage; // Le composant texte de ton Canvas
    public Vector3 offset = new Vector3(0, 0.5f, 0); // Pour afficher le texte un peu au-dessus de l'objet

    private void Awake()
    {
        // Création du Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // On le cache au démarrage
        gameObject.SetActive(false);
    }

    public void AfficherTooltip(string message, Transform positionObjet)
    {
        texteAffichage.text = message;
        // On déplace le Canvas au-dessus de l'objet
        transform.position = positionObjet.position + offset;
        gameObject.SetActive(true);
    }

    public void MasquerTooltip()
    {
        gameObject.SetActive(false);
    }
}