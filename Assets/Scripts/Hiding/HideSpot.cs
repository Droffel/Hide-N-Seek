using UnityEngine;

public class HideSpot : MonoBehaviour
{

    [SerializeField] private bool canHide = true;


    private bool isOccupied = false;

    // checks if item is hideable
    public bool CanHide()
    {
        return canHide;
    }

    // check if another player is in there
    public bool IsOccupied()
    {
        return isOccupied;
    }

    // if player can hide or not (no if another player is in it)
    public bool TryOccupy()
    {
        if (!canHide || isOccupied)
        {
            return false;
        }

        isOccupied = true;
        return true;
    }

    // when player leaves item
    public void Release()
    {
        isOccupied = false;
    }
}