using UnityEngine;
using TMPro; // Pour le TextMeshPro

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI")]
    public TextMeshProUGUI texteAffichage; // Le composant texte de ton Canvas
    public Vector3 offset = new Vector3(0, 0.5f, 0); // Pour afficher le texte un peu au-dessus de l'objet

    [SerializeField] private GameObject canvas;
    [SerializeField] private ProgressBar healthBar;

    private void Awake()
    {
        // Création du Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // On le cache au démarrage
        //gameObject.SetActive(false);
        MasquerTooltip();
    }

    public void AfficherTooltip(string message, float health, bool isSwale, Transform positionObjet)
    {
        texteAffichage.text = message;
        canvas.SetActive(true);
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(isSwale);
            if (isSwale) healthBar.BarValue = health;
        }

        // On déplace le Canvas au-dessus de l'objet
        transform.position = positionObjet.position + offset;
        //gameObject.SetActive(true);
        //canvas.SetActive(true);
    }

    public void MasquerTooltip()
    {
        //gameObject.SetActive(false);
        canvas.SetActive(false);
    }
}