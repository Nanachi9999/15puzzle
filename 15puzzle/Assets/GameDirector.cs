using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GameDirector : MonoBehaviour
{
    //�萔
    private const int BOARD_X = 4;
    private const int BOARD_Y = 4;
    private const float DISTANCE_FROM_CENTER = 1.5f;

    //�v���n�u�Z�b�g
    public List<GameObject> prefabTile_List;

    //�{�[�h�z��
    public TileController[,] tileController_Array = new TileController[BOARD_X, BOARD_Y];

    // Start is called before the first frame update
    void Start()
    {
        //�^�C���I�u�W�F�N�g�z�u
        SetTiles();

        ShuffleBoard();
    }

    // Update is called once per frame
    void Update()
    {
        //�N���b�N������^�C������ւ������݂�
        if (Input.GetMouseButtonUp(0))
        {
            //�^�C���擾
            TileController selectedTile = GetTile();

            if (selectedTile != null)
            {
                Debug.Log(selectedTile.myNumber + "�Ԃ̃}�X�������ꂽ��");
                MoveTile(selectedTile);
                selectedTile = null;

                if (IsCleared(tileController_Array))
                {
                    Debug.Log("����");
                }
            }
            else
            {
                Debug.Log("�}�X�����ĂȂ��ł�");
            }
        }
    }

    void SetTiles()
    {
        int count = 0;
        Vector3 worldPos;

        for (int i = 0; i < BOARD_X; i++)
        {
            for (int j = 0; j < BOARD_Y; j++)
            {
                //�Ō�̃}�X�����
                if (count == prefabTile_List.Count)
                {
                    tileController_Array[i, j] = null;
                    return;
                }

                //���W����
                worldPos = new(DISTANCE_FROM_CENTER - j, 0, -DISTANCE_FROM_CENTER + i);

                //�v���n�u����
                GameObject currentTile = Instantiate(prefabTile_List[count], worldPos, Quaternion.identity);
                TileController currentTileCtrl = currentTile.GetComponent<TileController>();

                //�z��Ɋi�[
                tileController_Array[i,j] = currentTileCtrl;
                //TileController�̏�����
                currentTileCtrl.initTile(new Vector2Int(i, j), count);

                count++;
            }
        }
    }

    TileController GetTile()
    {
        TileController ret = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            if (hit.collider.CompareTag("Player"))
            { 
                ret = hit.transform.gameObject.GetComponent<TileController>();
                break;
            }
        }
        
        return ret;
    }

    void MoveTile(TileController selected)
    {
        Vector2Int index =  GetMovableTile(selected);

        tileController_Array[selected.pos.x, selected.pos.y] = null; //�������ʒu��null��

        tileController_Array[index.x, index.y] = selected; //�s�����selected��

        selected.UpdatePos(index);
    }

    Vector2Int GetMovableTile(TileController tile)
    {
        //�E
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y + 1)) //�z����Ȃ璲�ׂ�
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y + 1]) == null)
            {
                //Debug.Log("�E�ɓ�������");
                return new Vector2Int(tile.pos.x, tile.pos.y + 1);
            }
        }

        //��
        if (IsInBoard(tileController_Array, tile.pos.x + 1, tile.pos.y)) //�z����Ȃ璲�ׂ�
        {
            if ((tileController_Array[tile.pos.x + 1, tile.pos.y]) == null)
            {
                //Debug.Log("���ɓ�������");
                return new Vector2Int(tile.pos.x + 1, tile.pos.y);
            }
        }

        //��
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y - 1)) //�z����Ȃ璲�ׂ�
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y - 1]) == null)
            {
                //Debug.Log("���ɓ�������");
                return new Vector2Int(tile.pos.x, tile.pos.y - 1);
            }
        }

        //��
        if (IsInBoard(tileController_Array, tile.pos.x - 1, tile.pos.y)) //�z����Ȃ璲�ׂ�
        {
            if ((tileController_Array[tile.pos.x - 1, tile.pos.y]) == null)
            {
                //Debug.Log("��ɓ�������");
                return new Vector2Int(tile.pos.x - 1, tile.pos.y);
            }
        }

        //Debug.Log("�S�R�����Ȃ����e�F");
        return new Vector2Int(tile.pos.x, tile.pos.y); //���̏ꂩ�瓮���Ȃ�
    }
    
    bool IsInBoard(TileController[,] array, int x, int y) //�z��O�łȂ����m���߂�
    {
        if (x < 0 ||
            array.GetLength(0) <= x ||
            y < 0 ||
            array.GetLength(1) <= y)
        {
            return false; //�z��̊O�ɏo�Ă�����false��Ԃ�
        }

        return true; //��L�̏����ɓ��Ă͂܂�Ȃ����true
    }

    bool IsCleared(TileController[,] array)
    {
        int previousNum = 0;
        Vector2Int end = new Vector2Int(BOARD_X - 1, BOARD_Y - 1); //�Ō�̃}�X

        if (array[end.x, end.y] != null)
        {
            return false;
        }

        for (int i = 0; i < BOARD_X; i++) //����
        {
            for (int j = 0; j < BOARD_Y; j++)
            {
                if ((i == end.x) && (j == end.y - 1))
                { 
                    return true; //�ŏI�}�X�̓`�F�b�N�ςȂ̂ŁA���̂ЂƂO�܂ł��ǂ蒅������N���A
                }

                if (array[i, j].myNumber < previousNum)
                {
                    return false; //��O�̃}�X��萔�����傫���Ȃ��Ȃ�N���A����Ȃ�
                }
            }
        }

        return false;
    }

    async void ShuffleBoard()
    {
        Vector2Int nullPos = new Vector2Int(BOARD_X - 1, BOARD_Y - 1); //�{�[�h�̒���null�ɂȂ��Ă���ʒu
        Vector2Int oldPos = new Vector2Int(BOARD_X, BOARD_Y); //�O�̃^�[����null�������ʒu�i�O�̃^�[���ŐG�����}�X�j
        Vector2Int movePos; //�G��}�X

        List<Vector2Int> movableTiles_List = new List<Vector2Int>(); //null�}�X�ɐڂ��Ă��āA����oldPos�łȂ��}�X���i�[���郊�X�g

        //100�񃉃��_���ɓ�����
        for (int i = 0; i < 100; i++)
        {
            movableTiles_List.Clear(); //���X�g�N���A

            //null�}�X����݂ĉE
            if (IsInBoard(tileController_Array, nullPos.x, nullPos.y + 1)) //�z����Ȃ璲�ׂ�
            {
                if (new Vector2Int(nullPos.x, nullPos.y + 1) != oldPos) //�������O�̃^�[���ŐG�����}�X�ł͂Ȃ��Ȃ�
                {
                    Debug.Log("�E��ǉ�");
                    movableTiles_List.Add(new Vector2Int(nullPos.x, nullPos.y + 1)); //���ɐG��}�X�̌��ɂ���
                }
            }

            //null�}�X����݂ĉ�
            if (IsInBoard(tileController_Array, nullPos.x + 1, nullPos.y)) //�z����Ȃ璲�ׂ�
            {
                if (new Vector2Int(nullPos.x + 1, nullPos.y) != oldPos) //�������O�̃^�[���ŐG�����}�X�ł͂Ȃ��Ȃ�
                {
                    Debug.Log("����ǉ�");
                    movableTiles_List.Add(new Vector2Int(nullPos.x + 1, nullPos.y)); //���ɐG��}�X�̌��ɂ���
                }
            }

            //null�}�X����݂č�
            if (IsInBoard(tileController_Array, nullPos.x, nullPos.y - 1)) //�z����Ȃ璲�ׂ�
            {
                if (new Vector2Int(nullPos.x, nullPos.y - 1) != oldPos) //�������O�̃^�[���ŐG�����}�X�ł͂Ȃ��Ȃ�
                {
                    Debug.Log("����ǉ�");
                    movableTiles_List.Add(new Vector2Int(nullPos.x, nullPos.y - 1)); //���ɐG��}�X�̌��ɂ���
                }
            }

            //null�}�X����݂ĉ�
            if (IsInBoard(tileController_Array, nullPos.x - 1, nullPos.y)) //�z����Ȃ璲�ׂ�
            {
                if(new Vector2Int(nullPos.x - 1, nullPos.y) != oldPos) //�������O�̃^�[���ŐG�����}�X�ł͂Ȃ��Ȃ�
                {
                    Debug.Log("���ǉ�");
                    movableTiles_List.Add(new Vector2Int(nullPos.x - 1, nullPos.y)); //���ɐG��}�X�̌��ɂ���
                }
            }

            //�����܂łŎ��ɐG���}�X�����X�g�ɍŒ�ЂƂ͓����Ă���̂Ń����_���ɂЂƂ��o���āA�G��
            int rnd = Random.Range(0, movableTiles_List.Count);
            Debug.Log(rnd);
            movePos = movableTiles_List[rnd];

            MoveTile(tileController_Array[movePos.x, movePos.y]);
            oldPos = nullPos;
            nullPos = movePos;

            await Task.Delay(20);
        }

    }
}
