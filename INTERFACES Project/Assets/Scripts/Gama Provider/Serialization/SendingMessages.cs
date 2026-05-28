using UnityEngine;
using TMPro;

public class SendingMessages : MonoBehaviour
{
    public static SendingMessages Instance;
    public TextMeshPro text;

    void Awake()
    {
        Instance = this;
    }

    public static void Show(string msg)
    {
        Instance.text.text = msg;
    }
}

