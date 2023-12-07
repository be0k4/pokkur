using System;

/// <summary>
/// 全アイテム共通のインターフェイス
/// <para>比較可能</para>
/// </summary>
public interface ICollectable : IDataPersistence, IComparable<ICollectable>
{
    //ICollectable型でアイテムをまとめて扱うが、インターフェイスではフィールドを持てないので、継承先のフィールドにアクセスするためのメソッドを用意
    ItemData GetItemData();
    //取得時の処理
    //当たり判定で、プレイヤー側からこのメソッドを呼び出す
    void Collect();
    //インベントリから外に出した時の処理
    void Instatiate();
    //リポップ制御で管理するIDを生成する
    //継承先のクラスで実装時に[ContextMenu]をつけて、インスペクタからIDを生成できるようにする
    void GenerateGuid();
}
