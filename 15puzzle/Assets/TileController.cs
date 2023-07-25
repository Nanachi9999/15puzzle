using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private const float DISTANCE_FROM_CENTER = 1.5f;

    public Vector2Int pos;
    public int myNumber;

    //èâä˙âª
    public TileController initTile(Vector2Int POS, int NUM)
    {
        pos = POS;
        myNumber = NUM;

        return this;
    }

    public void UpdatePos(Vector2Int newPos)
    {
        pos.x = newPos.x;
        pos.y = newPos.y;

        transform.position = new Vector3(DISTANCE_FROM_CENTER - newPos.y, 0.0f, -DISTANCE_FROM_CENTER + newPos.x);
    }
}
