using System.Collections.Generic; // Indispensable pour utiliser les Listes
using UnityEngine;

public class FlowArrowMulti : MonoBehaviour
{
    // Cette petite structure permet de grouper un point de départ et de fin
    [System.Serializable]
    public struct ArrowData
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
    }

    [Header("Liste des Flèches")]
    public List<ArrowData> arrowsData = new List<ArrowData>();

    [Header("Apparence Globale")]
    public float shaftRadius = 0.3f;
    public float headRadius = 0.6f;
    public float headHeight = 1f;
    public Color arrowColor = Color.blue;

    [Header("Animation")]
    public float waveAmplitude = 0.5f;
    public float waveSpeed = 2f;

    // Listes pour stocker les objets 3D générés
    private List<GameObject> shafts = new List<GameObject>();
    private List<GameObject> heads = new List<GameObject>();

    void Start()
    {
        // On boucle sur chaque flèche que vous avez configurée dans l'inspecteur
        for (int i = 0; i < arrowsData.Count; i++)
        {
            // 1. Création du Corps (Cylinder)
            GameObject newShaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            newShaft.transform.SetParent(transform);
            newShaft.GetComponent<Renderer>().material.color = arrowColor;

            // Sécurité VR : On détruit le collider pour que le laser passe au travers
            Destroy(newShaft.GetComponent<Collider>());

            shafts.Add(newShaft); // On le sauvegarde dans notre liste

            // 2. Création de la Tête
            GameObject newHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            newHead.transform.SetParent(transform);
            newHead.GetComponent<Renderer>().material.color = arrowColor;
            Destroy(newHead.GetComponent<Collider>());

            heads.Add(newHead); // On le sauvegarde dans notre liste
        }
    }

    void Update()
    {
        // S'il n'y a pas de flèches, on ne fait rien
        if (arrowsData.Count > 0)
        {
            UpdateArrows();
        }
    }

    void UpdateArrows()
    {
        float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmplitude;
        float fluxProgression = (Time.time * waveSpeed * 0.1f) % 1.0f;

        // On anime toutes les flèches de la liste en même temps
        for (int i = 0; i < arrowsData.Count; i++)
        {
            Vector3 start = arrowsData[i].startPoint;
            Vector3 end = arrowsData[i].endPoint;
            Vector3 dir = end - start;
            float totalLength = dir.magnitude;

            // --- ANIMATION DE L'ARRIÈRE VERS L'AVANT ---
            // Pour donner l'effet que le flux avance, on décale légèrement 
            // le point de départ et le point d'arrivée dans le sens de la direction.
            Vector3 directionNorm = dir.normalized;

            // On fait osciller la longueur du corps pour donner un effet de pompe/impulsion vers l'avant
            float impulsionLongueur = Mathf.Sin((Time.time * waveSpeed) - (i * 0.5f)) * waveAmplitude;

            // Le centre se déplace légèrement vers l'avant au rythme de la vague
            Vector3 center = (start + end) / 2f + (directionNorm * impulsionLongueur);

            // Mise à jour du Corps (il s'allonge et se rétracte vers l'avant)
            shafts[i].transform.position = center;
            shafts[i].transform.up = directionNorm;

            // On applique la variation sur la longueur (axe Y du cylindre)
            float nouvelleLongueur = totalLength + (impulsionLongueur * 2f);
            shafts[i].transform.localScale = new Vector3(shaftRadius, nouvelleLongueur / 2f, shaftRadius);

            // Mise à jour de la Tête (elle reste au bout mais pulse en taille pour marquer le flux arrivant)
            float pulsationTete = headRadius + (Mathf.Sin(Time.time * waveSpeed + 1.5f) * waveAmplitude * 0.2f);
            heads[i].transform.position = end + (directionNorm * impulsionLongueur * 0.5f);
            heads[i].transform.up = directionNorm;
            heads[i].transform.localScale = new Vector3(pulsationTete, headHeight, pulsationTete);

            /* pour ondulations verticales */

            //Vector3 animStart = arrowsData[i].startPoint + Vector3.up * wave;
            //Vector3 animEnd = arrowsData[i].endPoint + Vector3.up * wave;

            //Vector3 dir = animEnd - animStart;
            //float length = dir.magnitude;
            //Vector3 center = (animStart + animEnd) / 2f;

            //// Mise à jour du Corps
            //shafts[i].transform.position = center;
            //shafts[i].transform.up = dir.normalized;
            //shafts[i].transform.localScale = new Vector3(shaftRadius, length / 2f, shaftRadius);

            //// Mise à jour de la Tête
            //heads[i].transform.position = animEnd;
            //heads[i].transform.up = dir.normalized;
            //heads[i].transform.localScale = new Vector3(headRadius, headHeight, headRadius);
        }
    }
}
