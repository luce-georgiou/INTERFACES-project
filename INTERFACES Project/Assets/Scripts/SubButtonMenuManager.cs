using UnityEngine;

public class GestionnaireSousMenu : MonoBehaviour
{
    [Header("Objet contenant les 2 boutons")]
    [SerializeField] private GameObject conteneurSousBoutons;

    // Cette fonction sera appelée quand on clique sur le bouton principal
    public void BasculerAffichage()
    {
        if (conteneurSousBoutons != null)
        {
            // On regarde si le conteneur est actuellement allumé ou éteint
            bool estActif = conteneurSousBoutons.activeSelf;

            // On inverse son état (True devient False, False devient True)
            conteneurSousBoutons.SetActive(!estActif);
        }
        else
        {
            Debug.LogWarning("N'oublie pas d'assigner le conteneur dans l'inspecteur !");
        }
    }
}
