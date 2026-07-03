using UnityEngine;

public class ModifyLocalFlora : MonoBehaviour
{
    void Start()
    {
        // 1. On l'agrandit dès sa naissance (Le Scale ne change pas dans GAMA en général, donc Start suffit)
        transform.localScale = new Vector3(2f, 2f, 2f);
    }

    void LateUpdate()
    {
        // 2. On force la hauteur à -3f dans le LateUpdate.
        // LateUpdate s'exécute APRES que le script de GAMA a déplacé l'objet.
        // Cela permet de "gagner le duel" contre GAMA si celui-ci essaie d'écraser la position.
        transform.position = new Vector3(transform.position.x, -1.5f, transform.position.z);
    }
}
