using UnityEngine;
using TMPro; // Pour le TextMeshPro

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI")]
    public TextMeshProUGUI texteAffichage; // Le composant texte de ton Canvas
    public Vector3 offset = new Vector3(0, 0.5f, 0); // Pour afficher le texte un peu au-dessus du point visé

    [Tooltip("Distance pour repousser le texte afin qu'il ne soit pas collé aux yeux du joueur en VR.")]
    public float reculCamera = 0.6f; // <-- VARIABLE PAR DEFAUT

    [SerializeField] private GameObject canvas;
    [SerializeField] private ProgressBar healthBar;

    private void Awake()
    {
        // Création du Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // On le cache au démarrage
        MasquerTooltip();
    }

    // AJOUT : Le paramètre "float? customReculCamera = null" permet de passer un recul personnalisé.
    public void AfficherTooltip(string message, float health, bool isSwale, Transform positionObjet, Vector3? customOffset = null, float? customReculCamera = null)
    {
        // 1. Mise à jour des informations UI
        texteAffichage.text = message;
        canvas.SetActive(true);
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(isSwale);
            if (isSwale) healthBar.BarValue = health;
        }

        // 2. Choix de l'offset et du recul : si les variables custom ont une valeur, on les utilise, sinon on utilise les valeurs par défaut.
        Vector3 offsetApplique = customOffset ?? offset;
        float reculApplique = customReculCamera ?? reculCamera;

        // --- DEBUT SOLUTION 2 : POSITIONNEMENT INTELLIGENT ---

        // A. On récupère la position du joueur (Assure-toi que ta caméra VR a bien le tag "MainCamera")
        Vector3 positionJoueur = Camera.main.transform.position;

        // B. On cherche le Collider de l'objet visé (ou de ses enfants)
        Collider colObjet = positionObjet.GetComponentInChildren<Collider>();

        if (colObjet != null)
        {
            // C. On calcule le point du collider le plus proche du joueur
            Vector3 pointPlusProche = colObjet.ClosestPoint(positionJoueur);

            // NOUVEAU : On calcule une direction HORIZONTALE pour éloigner le menu du joueur
            // On met la position du joueur à la même hauteur (Y) que le point pour ne pas repousser le menu dans le sol
            Vector3 positionJoueurNiveauPoint = new Vector3(positionJoueur.x, pointPlusProche.y, positionJoueur.z);
            Vector3 directionHorizontale = (pointPlusProche - positionJoueurNiveauPoint).normalized;

            // Si le joueur est parfaitement au-dessus du point (direction nulle), on utilise la direction de son regard
            if (directionHorizontale == Vector3.zero)
            {
                directionHorizontale = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            }

            // D. On place le Canvas à ce point + notre offset appliqué + le recul appliqué pour le confort des yeux
            transform.position = pointPlusProche + offsetApplique + (directionHorizontale * reculApplique);
        }
        else
        {
            // Sécurité : si l'objet n'a pas de collider, on garde l'ancienne méthode avec le recul
            Vector3 positionJoueurNiveauPoint = new Vector3(positionJoueur.x, positionObjet.position.y, positionJoueur.z);
            Vector3 directionHorizontale = (positionObjet.position - positionJoueurNiveauPoint).normalized;
            if (directionHorizontale == Vector3.zero) directionHorizontale = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;

            transform.position = positionObjet.position + offsetApplique + (directionHorizontale * reculApplique);
        }

        // E. On fait tourner le Tooltip pour qu'il regarde le joueur
        // L'axe Z du canvas va pointer à l'opposé du joueur, donc le texte sera lisible
        transform.rotation = Quaternion.LookRotation(transform.position - positionJoueur);

        // F. On bloque la rotation sur l'axe X et Z 
        // pour que le menu reste droit et ne penche pas vers le haut/bas si le joueur est plus petit ou plus grand.
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // --- FIN SOLUTION 2 ---
    }

    public void MasquerTooltip()
    {
        canvas.SetActive(false);
    }
}