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

    //プレハブをセットするリスト。インスペクターから設定
    public List<GameObject> prefabTile_List;

    //タイルの配列
    public TileController[,] tileController_Array = new TileController[BOARD_X, BOARD_Y];

    void Start()
    {
        //タイルオブジェクト配置
        SetTiles();

        //生成したタイルをランダムに動かして混ぜる（ランダムに配置すると詰むので）
        ShuffleBoard();
    }

    void Update()
    {
        //クリックしたらタイル入れ替えを試みる
        if (Input.GetMouseButtonUp(0))
        {
            //タイル取得
            TileController selectedTile = GetTile();

            //取得に成功したら
            if (selectedTile != null)
            {
                Debug.Log(selectedTile.myNumber + "番のマスが押されたよ");

                //取得したタイルを移動
                MoveTile(selectedTile);
                selectedTile = null;

                //クリアチェック
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

    //1~15のタイルを順番通りに配置
    void SetTiles()
    {
        //ループ回数のカウント
        int count = 0;
        //生成する位置
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

                //座標決定　原点から一定距離離れた位置から生成を開始することで盤面がワールドの中央に来る
                worldPos = new(DISTANCE_FROM_CENTER - j, 0, -DISTANCE_FROM_CENTER + i);

                //リストから選んだプレハブを生成
                GameObject currentTile = Instantiate(prefabTile_List[count], worldPos, Quaternion.identity);
                //生成したプレハブからTileControllerを取得
                TileController currentTileCtrl = currentTile.GetComponent<TileController>();

                //配列に格納
                tileController_Array[i,j] = currentTileCtrl;
                //TileControllerの初期化
                currentTileCtrl.initTile(new Vector2Int(i, j), count);

                //ループ回数をカウント
                count++;
            }
        }
    }

    //クリックしたタイルを取得
    TileController GetTile()
    {
        //返却用
        TileController ret = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //rayが振れたすべてのオブジェクトの中からPlayerタグがついたオブジェクトを見つけたら返り値にする
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

    //タイルを移動させる
    void MoveTile(TileController selected)
    {
        //移動すべき場所を調べる
        Vector2Int index =  GetMovableTile(selected);

        //元居た位置をnullに
        tileController_Array[selected.pos.x, selected.pos.y] = null;
        //行き先をselectedに
        tileController_Array[index.x, index.y] = selected;

        //ワールド座標を更新
        selected.UpdatePos(index);
    }

    Vector2Int GetMovableTile(TileController tile)
    {
        //右
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y + 1)) //配列内なら調べる
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y + 1]) == null) //そこがnullなら(移動可能な場所なら)
            {
                //Debug.Log("右に動けそう");
                return new Vector2Int(tile.pos.x, tile.pos.y + 1);
            }
        }

        //下
        if (IsInBoard(tileController_Array, tile.pos.x + 1, tile.pos.y))
        {
            if ((tileController_Array[tile.pos.x + 1, tile.pos.y]) == null)
            {
                //Debug.Log("下に動けそう");
                return new Vector2Int(tile.pos.x + 1, tile.pos.y);
            }
        }

        //左
        if (IsInBoard(tileController_Array, tile.pos.x, tile.pos.y - 1))
        {
            if ((tileController_Array[tile.pos.x, tile.pos.y - 1]) == null)
            {
                //Debug.Log("左に動けそう");
                return new Vector2Int(tile.pos.x, tile.pos.y - 1);
            }
        }

        //上
        if (IsInBoard(tileController_Array, tile.pos.x - 1, tile.pos.y))
        {
            if ((tileController_Array[tile.pos.x - 1, tile.pos.y]) == null)
            {
                //Debug.Log("上に動けそう");
                return new Vector2Int(tile.pos.x - 1, tile.pos.y);
            }
        }

        //Debug.Log("全然動けなくっテェ");
        return tile.pos; //その場から動かない
    }
    
    bool IsInBoard(TileController[,] array, int x, int y) //配列外でないか確かめる
    {
        if (x < 0 ||
            array.GetLength(0) <= x ||
            y < 0 ||
            array.GetLength(1) <= y)
        {
            return false; //配列の外に出ていたらfalse
        }

        return true; //上記の条件に当てはまらなければtrue
    }

    //クリア判定
    bool IsCleared(TileController[,] array)
    {
        //ループ回数をカウント
        int count = 0;
        //ループの上限回数
        int end = BOARD_X * BOARD_Y - 1;
        //ひとつ前に調べたマスの番号
        int previousNum = 0;

        //最後のマスがnullでないなら絶対にクリアされていない
        if (array[BOARD_X - 1, BOARD_Y - 1] != null)
        {
            return false;
        }

        for (int i = 0; i < BOARD_X; i++) //走査
        {
            for (int j = 0; j < BOARD_Y; j++)
            {
                Debug.Log(count);

                if (array[i, j].myNumber < previousNum)
                {
                    return false; //一つ前のマス寄り数字が大きくないならクリアじゃない
                }

                previousNum = array[i, j].myNumber;
                count++;

                //最後のマスを調べてしまうとnullにアクセスしてしまうので調べない
                if (count == end) break;
            }
        }

        //上記のチェックを通過したならクリア
        return true;
    }

    //マスをランダムに動かして混ぜる
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
            if (IsInBoard(tileController_Array, nullPos.x + 1, nullPos.y))
            {
                if (new Vector2Int(nullPos.x + 1, nullPos.y) != oldPos)
                {
                    Debug.Log("下を追加");
                    movableTiles_List.Add(new Vector2Int(nullPos.x + 1, nullPos.y));
                }
            }

            //nullマスからみて左
            if (IsInBoard(tileController_Array, nullPos.x, nullPos.y - 1))
            {
                if (new Vector2Int(nullPos.x, nullPos.y - 1) != oldPos)
                {
                    Debug.Log("左を追加");
                    movableTiles_List.Add(new Vector2Int(nullPos.x, nullPos.y - 1));
                }
            }

            //nullマスからみて下
            if (IsInBoard(tileController_Array, nullPos.x - 1, nullPos.y))
            {
                if(new Vector2Int(nullPos.x - 1, nullPos.y) != oldPos)
                {
                    Debug.Log("上を追加");
                    movableTiles_List.Add(new Vector2Int(nullPos.x - 1, nullPos.y));
                }
            }

            //ここまでで次に触れるマスがリストに最低ひとつは入っているのでランダムにひとつ取り出して、触る
            int rnd = Random.Range(0, movableTiles_List.Count);
            Debug.Log(rnd);
            movePos = movableTiles_List[rnd];

            MoveTile(tileController_Array[movePos.x, movePos.y]);

            //nullだったマスの位置をoldPosに格納し、触ったマスの位置をnullPosに格納
            oldPos = nullPos;
            nullPos = movePos;

            //20ミリ秒待つ（シャッフル演出用）
            await Task.Delay(20);
        }

    }
}
