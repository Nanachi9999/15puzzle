using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GameDirector : MonoBehaviour
{
    //定数
    private const int BOARD_X = 4;
    private const int BOARD_Y = 4;
    private const float DISTANCE_FROM_CENTER = 1.5f;

    //プレハブセット
    public List<GameObject> prefabTile_List;

    //ボード配列
    public TileController[,] tileController_Array = new TileController[BOARD_X, BOARD_Y];

    // Start is called before the first frame update
    void Start()
    {
        //タイルオブジェクト配置
        SetTiles();

        ShuffleBoard();
    }

    // Update is called once per frame
    void Update()
    {
        //クリックしたらタイル入れ替えを試みる
        if (Input.GetMouseButtonUp(0))
        {
            //タイル取得
            TileController selectedTile = GetTile();

            if (selectedTile != null)
            {
                Debug.Log(selectedTile.myNumber + "番のマスが押されたよ");
                MoveTile(selectedTile);
                selectedTile = null;

                if (IsCleared(tileController_Array))
                {
                    Debug.Log("おめ");
                }
            }
            else
            {
                Debug.Log("マス押せてないです");
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
                //最後のマスを空に
                if (count == prefabTile_List.Count)
                {
                    tileController_Array[i, j] = null;
                    return;
                }

                //座標決定
                worldPos = new(DISTANCE_FROM_CENTER - j, 0, -DISTANCE_FROM_CENTER + i);

                //プレハブ生成
                GameObject currentTile = Instantiate(prefabTile_List[count], worldPos, Quaternion.identity);
                TileController currentTileCtrl = currentTile.GetComponent<TileController>();

                //配列に格納
                tileController_Array[i,j] = currentTileCtrl;
                //TileControllerの初期化
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

        tileController_Array[selected.pos.x, selected.pos.y] = null; //元居た位置をnullに

        tileController_Array[index.x, index.y] = selected; //行き先をselectedに

        selected.UpdatePos(index);
    }

    Vector2Int GetMovableTile(TileController tile)
    {
        //右
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y + 1)) //配列内なら調べる
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y + 1]) == null)
            {
                //Debug.Log("右に動けそう");
                return new Vector2Int(tile.pos.x, tile.pos.y + 1);
            }
        }

        //下
        if (IsInBoard(tileController_Array, tile.pos.x + 1, tile.pos.y)) //配列内なら調べる
        {
            if ((tileController_Array[tile.pos.x + 1, tile.pos.y]) == null)
            {
                //Debug.Log("下に動けそう");
                return new Vector2Int(tile.pos.x + 1, tile.pos.y);
            }
        }

        //左
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y - 1)) //配列内なら調べる
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y - 1]) == null)
            {
                //Debug.Log("左に動けそう");
                return new Vector2Int(tile.pos.x, tile.pos.y - 1);
            }
        }

        //上
        if (IsInBoard(tileController_Array, tile.pos.x - 1, tile.pos.y)) //配列内なら調べる
        {
            if ((tileController_Array[tile.pos.x - 1, tile.pos.y]) == null)
            {
                //Debug.Log("上に動けそう");
                return new Vector2Int(tile.pos.x - 1, tile.pos.y);
            }
        }

        //Debug.Log("全然動けなくっテェ");
        return new Vector2Int(tile.pos.x, tile.pos.y); //その場から動かない
    }
    
    bool IsInBoard(TileController[,] array, int x, int y) //配列外でないか確かめる
    {
        if (x < 0 ||
            array.GetLength(0) <= x ||
            y < 0 ||
            array.GetLength(1) <= y)
        {
            return false; //配列の外に出ていたらfalseを返す
        }

        return true; //上記の条件に当てはまらなければtrue
    }

    bool IsCleared(TileController[,] array)
    {
        int previousNum = 0;
        Vector2Int end = new Vector2Int(BOARD_X - 1, BOARD_Y - 1); //最後のマス

        if (array[end.x, end.y] != null)
        {
            return false;
        }

        for (int i = 0; i < BOARD_X; i++) //走査
        {
            for (int j = 0; j < BOARD_Y; j++)
            {
                if ((i == end.x) && (j == end.y - 1))
                { 
                    return true; //最終マスはチェック済なので、そのひとつ前までたどり着いたらクリア
                }

                if (array[i, j].myNumber < previousNum)
                {
                    return false; //一つ前のマス寄り数字が大きくないならクリアじゃない
                }
            }
        }

        return false;
    }

    async void ShuffleBoard()
    {
        Vector2Int nullPos = new Vector2Int(BOARD_X - 1, BOARD_Y - 1); //ボードの中でnullになっている位置
        Vector2Int oldPos = new Vector2Int(BOARD_X, BOARD_Y); //前のターンでnullだった位置（前のターンで触ったマス）
        Vector2Int movePos; //触るマス

        List<Vector2Int> movableTiles_List = new List<Vector2Int>(); //nullマスに接していて、かつoldPosでないマスを格納するリスト

        //100回ランダムに動かす
        for (int i = 0; i < 100; i++)
        {
            movableTiles_List.Clear(); //リストクリア

            //nullマスからみて右
            if (IsInBoard(tileController_Array, nullPos.x, nullPos.y + 1)) //配列内なら調べる
            {
                if (new Vector2Int(nullPos.x, nullPos.y + 1) != oldPos) //そこが前のターンで触ったマスではないなら
                {
                    Debug.Log("右を追加");
                    movableTiles_List.Add(new Vector2Int(nullPos.x, nullPos.y + 1)); //次に触るマスの候補にする
                }
            }

            //nullマスからみて下
            if (IsInBoard(tileController_Array, nullPos.x + 1, nullPos.y)) //配列内なら調べる
            {
                if (new Vector2Int(nullPos.x + 1, nullPos.y) != oldPos) //そこが前のターンで触ったマスではないなら
                {
                    Debug.Log("下を追加");
                    movableTiles_List.Add(new Vector2Int(nullPos.x + 1, nullPos.y)); //次に触るマスの候補にする
                }
            }

            //nullマスからみて左
            if (IsInBoard(tileController_Array, nullPos.x, nullPos.y - 1)) //配列内なら調べる
            {
                if (new Vector2Int(nullPos.x, nullPos.y - 1) != oldPos) //そこが前のターンで触ったマスではないなら
                {
                    Debug.Log("左を追加");
                    movableTiles_List.Add(new Vector2Int(nullPos.x, nullPos.y - 1)); //次に触るマスの候補にする
                }
            }

            //nullマスからみて下
            if (IsInBoard(tileController_Array, nullPos.x - 1, nullPos.y)) //配列内なら調べる
            {
                if(new Vector2Int(nullPos.x - 1, nullPos.y) != oldPos) //そこが前のターンで触ったマスではないなら
                {
                    Debug.Log("上を追加");
                    movableTiles_List.Add(new Vector2Int(nullPos.x - 1, nullPos.y)); //次に触るマスの候補にする
                }
            }

            //ここまでで次に触れるマスがリストに最低ひとつは入っているのでランダムにひとつ取り出して、触る
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
