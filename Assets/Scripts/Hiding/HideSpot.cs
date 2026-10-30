using UnityEngine;

public static class HideSpot
{
    public static bool IsHideable(Transform target, Transform possessableRoot)
    {
        if (target == null || possessableRoot == null)
        {
            return false;
        }

        return target == possessableRoot ||
               target.IsChildOf(possessableRoot);
    }
}