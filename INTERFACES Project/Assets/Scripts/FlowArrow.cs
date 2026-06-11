using UnityEngine;

public class FlowArrow : MonoBehaviour
{
    [Header("Points")]
    public Vector3 startPoint;
    public Vector3 endPoint;

    [Header("Apparence")]
    public float shaftRadius = 0.3f;
    public float headRadius = 0.6f;
    public float headHeight = 1f;
    public Color arrowColor = Color.blue;

    [Header("Animation")]
    public float waveAmplitude = 0.5f;
    public float waveSpeed = 2f;

    private GameObject shaft;
    private GameObject head;

    void Start()
    {
        // Corps (cylinder)
        shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.transform.SetParent(transform);
        shaft.GetComponent<Renderer>().material.color = arrowColor;

        // Tête (cylinder plus large et court)
        head = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        head.transform.SetParent(transform);
        head.GetComponent<Renderer>().material.color = arrowColor;

        UpdateArrow();
    }

    void Update()
    {
        UpdateArrow();
    }

    void UpdateArrow()
    {
        float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmplitude;
        Vector3 animStart = startPoint + Vector3.up * wave;
        Vector3 animEnd = endPoint + Vector3.up * wave;

        Vector3 dir = animEnd - animStart;
        float length = dir.magnitude;
        Vector3 center = (animStart + animEnd) / 2f;

        // Corps
        shaft.transform.position = center;
        shaft.transform.up = dir.normalized;
        shaft.transform.localScale = new Vector3(shaftRadius, length / 2f, shaftRadius);

        // Tête
        head.transform.position = animEnd;
        head.transform.up = dir.normalized;
        head.transform.localScale = new Vector3(headRadius, headHeight, headRadius);
    }
}
