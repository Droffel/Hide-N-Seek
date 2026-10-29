using UnityEngine;

public class PossessableRoot : MonoBehaviour
{
    public bool IsHideable(Transform target)
    {
        return HideSpot.IsHideable(target, transform);
    }
}
