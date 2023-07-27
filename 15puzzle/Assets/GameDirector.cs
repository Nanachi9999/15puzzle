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

    //�v���n�u���Z�b�g���郊�X�g�B�C���X�y�N�^�[����ݒ�
    public List<GameObject> prefabTile_List;

    //�^�C���̔z��
    public TileController[,] tileController_Array = new TileController[BOARD_X, BOARD_Y];

    void Start()
    {
        //�^�C���I�u�W�F�N�g�z�u
        SetTiles();

        //���������^�C���������_���ɓ������č�����i�����_���ɔz�u����Ƌl�ނ̂Łj
        ShuffleBoard();
    }

    void Update()
    {
        //�N���b�N������^�C������ւ������݂�
        if (Input.GetMouseButtonUp(0))
        {
            //�^�C���擾
            TileController selectedTile = GetTile();

            //�擾�ɐ���������
            if (selectedTile != null)
            {
                Debug.Log(selectedTile.myNumber + "�Ԃ̃}�X�������ꂽ��");

                //�擾�����^�C�����ړ�
                MoveTile(selectedTile);
                selectedTile = null;

                //�N���A�`�F�b�N
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

    //1~15�̃^�C�������Ԓʂ�ɔz�u
    void SetTiles()
    {
        //���[�v�񐔂̃J�E���g
        int count = 0;
        //��������ʒu
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

                //���W����@���_�����苗�����ꂽ�ʒu���琶�����J�n���邱�ƂŔՖʂ����[���h�̒����ɗ���
                worldPos = new(DISTANCE_FROM_CENTER - j, 0, -DISTANCE_FROM_CENTER + i);

                //���X�g����I�񂾃v���n�u�𐶐�
                GameObject currentTile = Instantiate(prefabTile_List[count], worldPos, Quaternion.identity);
                //���������v���n�u����TileController���擾
                TileController currentTileCtrl = currentTile.GetComponent<TileController>();

                //�z��Ɋi�[
                tileController_Array[i,j] = currentTileCtrl;
                //TileController�̏�����
                currentTileCtrl.initTile(new Vector2Int(i, j), count);

                //���[�v�񐔂��J�E���g
                count++;
            }
        }
    }

    //�N���b�N�����^�C�����擾
    TileController GetTile()
    {
        //�ԋp�p
        TileController ret = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //ray���U�ꂽ���ׂẴI�u�W�F�N�g�̒�����Player�^�O�������I�u�W�F�N�g����������Ԃ�l�ɂ���
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

    //�^�C�����ړ�������
    void MoveTile(TileController selected)
    {
        //�ړ����ׂ��ꏊ�𒲂ׂ�
        Vector2Int index =  GetMovableTile(selected);

        //�������ʒu��null��
        tileController_Array[selected.pos.x, selected.pos.y] = null;
        //�s�����selected��
        tileController_Array[index.x, index.y] = selected;

        //���[���h���W���X�V
        selected.UpdatePos(index);
    }

    Vector2Int GetMovableTile(TileController tile)
    {
        //�E
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y + 1)) //�z����Ȃ璲�ׂ�
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y + 1]) == null) //������null�Ȃ�(�ړ��\�ȏꏊ�Ȃ�)
            {
                //Debug.Log("�E�ɓ�������");
                return new Vector2Int(tile.pos.x, tile.pos.y + 1);
            }
        }

        //��
        if (IsInBoard(tileController_Array, tile.pos.x + 1, tile.pos.y))
        {
            if ((tileController_Array[tile.pos.x + 1, tile.pos.y]) == null)
            {
                //Debug.Log("���ɓ�������");
                return new Vector2Int(tile.pos.x + 1, tile.pos.y);
            }
        }

        //��
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y - 1))
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y - 1]) == null)
            {
                //Debug.Log("���ɓ�������");
                return new Vector2Int(tile.pos.x, tile.pos.y - 1);
            }
        }

        //��
        if (IsInBoard(tileController_Array, tile.pos.x - 1, tile.pos.y))
        {
            if ((tileController_Array[tile.pos.x - 1, tile.pos.y]) == null)
            {
                //Debug.Log("��ɓ�������");
                return new Vector2Int(tile.pos.x - 1, tile.pos.y);
            }
        }

        //Debug.Log("�S�R�����Ȃ����e�F");
        return tile.pos; //���̏ꂩ�瓮���Ȃ�
    }
    
    bool IsInBoard(TileController[,] array, int x, int y) //�z��O�łȂ����m���߂�
    {
        if (x < 0 ||
            array.GetLength(0) <= x ||
            y < 0 ||
            array.GetLength(1) <= y)
        {
            return false; //�z��̊O�ɏo�Ă�����false
        }

        return true; //��L�̏����ɓ��Ă͂܂�Ȃ����true
    }

    //�N���A����
    bool IsCleared(TileController[,] array)
    {
        //���[�v�񐔂��J�E���g
        int count = 0;
        //���[�v�̏����
        int end = BOARD_X * BOARD_Y - 1;
        //�ЂƂO�ɒ��ׂ��}�X�̔ԍ�
        int previousNum = 0;

        //�Ō�̃}�X��null�łȂ��Ȃ��΂ɃN���A����Ă��Ȃ�
        if (array[BOARD_X - 1, BOARD_Y - 1] != null)
        {
            return false;
        }

        for (int i = 0; i < BOARD_X; i++) //����
        {
            for (int j = 0; j < BOARD_Y; j++)
            {
                Debug.Log(count);

                if (array[i, j].myNumber < previousNum)
                {
                    return false; //��O�̃}�X��萔�����傫���Ȃ��Ȃ�N���A����Ȃ�
                }

                previousNum = array[i, j].myNumber;
                count++;

                //�Ō�̃}�X�𒲂ׂĂ��܂���null�ɃA�N�Z�X���Ă��܂��̂Œ��ׂȂ�
                if (count == end) break;
            }
        }

        //��L�̃`�F�b�N��ʉ߂����Ȃ�N���A
        return true;
    }

    //�}�X�������_���ɓ������č�����
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
            if (IsInBoard(tileController_Array, nullPos.x + 1, nullPos.y))
            {
                if (new Vector2Int(nullPos.x + 1, nullPos.y) != oldPos)
                {
                    Debug.Log("����ǉ�");
                    movableTiles_List.Add(new Vector2Int(nullPos.x + 1, nullPos.y));
                }
            }

            //null�}�X����݂č�
            if (IsInBoard(tileController_Array, nullPos.x, nullPos.y - 1))
            {
                if (new Vector2Int(nullPos.x, nullPos.y - 1) != oldPos)
                {
                    Debug.Log("����ǉ�");
                    movableTiles_List.Add(new Vector2Int(nullPos.x, nullPos.y - 1));
                }
            }

            //null�}�X����݂ĉ�
            if (IsInBoard(tileController_Array, nullPos.x - 1, nullPos.y))
            {
                if(new Vector2Int(nullPos.x - 1, nullPos.y) != oldPos)
                {
                    Debug.Log("���ǉ�");
                    movableTiles_List.Add(new Vector2Int(nullPos.x - 1, nullPos.y));
                }
            }

            //�����܂łŎ��ɐG���}�X�����X�g�ɍŒ�ЂƂ͓����Ă���̂Ń����_���ɂЂƂ��o���āA�G��
            int rnd = Random.Range(0, movableTiles_List.Count);
            Debug.Log(rnd);
            movePos = movableTiles_List[rnd];

            MoveTile(tileController_Array[movePos.x, movePos.y]);

            //null�������}�X�̈ʒu��oldPos�Ɋi�[���A�G�����}�X�̈ʒu��nullPos�Ɋi�[
            oldPos = nullPos;
            nullPos = movePos;

            //20�~���b�҂i�V���b�t�����o�p�j
            await Task.Delay(20);
        }

    }
}
