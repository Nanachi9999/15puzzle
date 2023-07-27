using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private const float DISTANCE_FROM_CENTER = 1.5f;

    //配列上の座標
    public Vector2Int pos;
    //マスの番号（ただし、１が描かれたタイルのmyNumberは0。15のタイルは14。）
    public int myNumber;

    //初期化
    public TileController initTile(Vector2Int newPos, int num)
    {
        pos = newPos;
        myNumber = num;

        return this;
    }

    //配列上の座標を受け取り、自身のワールド座標を更新する
    public void UpdatePos(Vector2Int newPos)
    {
        pos.x = newPos.x;
        pos.y = newPos.y;

        transform.position = new Vector3(DISTANCE_FROM_CENTER - newPos.y, 0.0f, -DISTANCE_FROM_CENTER + newPos.x);
    }
}
