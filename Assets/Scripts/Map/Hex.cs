using UnityEngine;

public class Hex : MonoBehaviour
{
    private GameObject prev = null;  // Reference to previous hex, null by default

    public void SetPrev(GameObject hex)
    {
        prev = hex;
    }

    public GameObject GetPrev()
    {
        return prev;
    }
} 