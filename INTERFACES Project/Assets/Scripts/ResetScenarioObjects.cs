using UnityEngine;
using System.Collections.Generic;

public class ResetScenarioObjects : MonoBehaviour
{
    public static ResetScenarioObjects Instance;
    // Structure pour stocker l'état d'origine d'un objet
    private class OriginalState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public bool isActive;
        public Color originalColor; // Si tu modifies les matériaux/couleurs
    }

    private Dictionary<GameObject, OriginalState> originalStates = new Dictionary<GameObject, OriginalState>();

    void Awake()
    {
        // Enregistrer l'état de tous les objets gérés au lancement de la scène (hors game time)
        SaveInitialStates();
    }

    public void SaveInitialStates()
    {
        originalStates.Clear();

        // Trouve tous les objets d'un tag spécifique ou d'une liste
        GameObject[] shrubsToTrack = GameObject.FindGameObjectsWithTag("shrubs_plants"); // Remplace par ton tag
        GameObject[] treesToTrack = GameObject.FindGameObjectsWithTag("tree");
        GameObject[] grassToTrack = GameObject.FindGameObjectsWithTag("grass");
        List<GameObject> listeConcatenee = new List<GameObject>(shrubsToTrack);
        listeConcatenee.AddRange(treesToTrack);
        listeConcatenee.AddRange(grassToTrack);

        foreach (GameObject obj in listeConcatenee)
        {
            OriginalState state = new OriginalState
            {
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale,
                isActive = obj.activeSelf
            };

            // Sauvegarder la couleur d'origine si l'objet a un Renderer
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                state.originalColor = rend.material.color;
            }

            originalStates.Add(obj, state);
        }

        Debug.Log($"{originalStates.Count} objets enregistrés dans leur état d'origine.");
    }

    /// <summary>
    /// Réinitialise tous les objets à l'état où ils étaient avant le début du jeu/scénario
    /// </summary>
    public void ResetToDefaultState()
    {
        foreach (KeyValuePair<GameObject, OriginalState> pair in originalStates)
        {
            GameObject obj = pair.Key;
            OriginalState state = pair.Value;

            if (obj != null)
            {
                obj.transform.position = state.position;
                obj.transform.rotation = state.rotation;
                obj.transform.localScale = state.scale;
                obj.SetActive(state.isActive);

                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null && rend.material != null)
                {
                    rend.material.color = state.originalColor;
                }
            }
        }

        Debug.Log("Tous les objets ont été réinitialisés à leur apparence hors-jeu.");
    }
}
