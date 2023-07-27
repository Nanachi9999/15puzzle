using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private const float DISTANCE_FROM_CENTER = 1.5f;

    //�z���̍��W
    public Vector2Int pos;
    //�}�X�̔ԍ��i�������A�P���`���ꂽ�^�C����myNumber��0�B15�̃^�C����14�B�j
    public int myNumber;

    //������
    public TileController initTile(Vector2Int newPos, int num)
    {
        pos = newPos;
        myNumber = num;

        return this;
    }

    //�z���̍��W���󂯎��A���g�̃��[���h���W���X�V����
    public void UpdatePos(Vector2Int newPos)
    {
        pos.x = newPos.x;
        pos.y = newPos.y;

        transform.position = new Vector3(DISTANCE_FROM_CENTER - newPos.y, 0.0f, -DISTANCE_FROM_CENTER + newPos.x);
    }
}
