using UnityEngine;
using TMPro; 

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI")]
    public TextMeshProUGUI texteAffichage; 
    public Vector3 offset = new Vector3(0, 0.5f, 0); 

    [Tooltip("Offset between text and camera")]
    public float reculCamera = 0.6f; 

    [SerializeField] private GameObject canvas;
    [SerializeField] private ProgressBar healthBar;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        MasquerTooltip();
    }

    public void AfficherTooltip(string message, float health, bool isSwale, Transform positionObjet, Vector3? customOffset = null, float? customReculCamera = null)
    {
        texteAffichage.text = message;
        canvas.SetActive(true);
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(isSwale);
            if (isSwale) healthBar.BarValue = health;
        }

        //Choosing offset
        Vector3 offsetApplique = customOffset ?? offset;
        float reculApplique = customReculCamera ?? reculCamera;

        //Player position
        Vector3 positionJoueur = Camera.main.transform.position;

        //Collider of selected object
        Collider colObjet = positionObjet.GetComponentInChildren<Collider>();

        if (colObjet != null)
        {
            //Look for closest point between collider and player
            Vector3 pointPlusProche = colObjet.ClosestPoint(positionJoueur);

            //adjust height
            Vector3 positionJoueurNiveauPoint = new Vector3(positionJoueur.x, pointPlusProche.y, positionJoueur.z);
            Vector3 directionHorizontale = (pointPlusProche - positionJoueurNiveauPoint).normalized;

            if (directionHorizontale == Vector3.zero)
            {
                directionHorizontale = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            }

            //Place canva
            transform.position = pointPlusProche + offsetApplique + (directionHorizontale * reculApplique);
        }
        else
        {
            //If no collider :
            Vector3 positionJoueurNiveauPoint = new Vector3(positionJoueur.x, positionObjet.position.y, positionJoueur.z);
            Vector3 directionHorizontale = (positionObjet.position - positionJoueurNiveauPoint).normalized;
            if (directionHorizontale == Vector3.zero) directionHorizontale = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;

            transform.position = positionObjet.position + offsetApplique + (directionHorizontale * reculApplique);
        }

        //Tooltip looks at player
        transform.rotation = Quaternion.LookRotation(transform.position - positionJoueur);

        //blocking rotation around x, z axis
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    public void MasquerTooltip()
    {
        canvas.SetActive(false);
    }
}