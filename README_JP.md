# Entities Events
 Provides inter-system messaging functionality to Unity ECS

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[English README is here](README.md)

## 概要

Entities EventsはUnityのEntity Component System(ECS)向けにイベント機能を追加するライブラリです。EventWriter/EventReaderを用いてSystem間のメッセージングを簡単に実装することができるようになります。

## 特徴

* EventWriter/EventReaderを用いた自然なSystem間メッセージング
* Events<T>を用いた独自のイベントシステムの作成

### 要件

* Unity 2022.3 以上
* Entities 1.0.0 以上

### インストール

1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力する

```
https://github.com/AnnulusGames/EntitiesEvents.git?path=Assets/EntitiesEvents
```

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記

```json
{
    "dependencies": {
        "com.annulusgames.entities-events": "https://github.com/AnnulusGames/EntitiesEvents.git?path=Assets/EntitiesEvents"
    }
}
```

## 基本的な使い方

Entities Eventsでは型をキーにしてイベントの書き込み/読み取りを行います。
まずはイベントに利用する構造体を定義します。イベントに用いる構造体に参照型を含めることはできず、unmanagedな型である必要があります。

```cs
public struct MyEvent { }
```

使用するイベントの型はあらかじめ`RegisterEvent`属性で登録しておく必要があります。この属性を付加することで、コンパイル時にSourceGeneratorが必要なSystemとassembly属性を含むコードを生成します。

```cs
using EntitiesEvents;

// アセンブリにRegisterEvent属性を追加
[assembly: RegisterEvent(typeof(MyEvent))]
```

送信側のSystemでは`EventWriter`を用いてイベントの発行を行います。

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial struct WriteEventSystem : ISystem
{
    // 取得したEventWriterはSystem内にキャッシュ
    EventWriter<MyEvent> eventWriter;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // GetEventWriterでEventWriterを取得
        eventWriter = state.GetEventWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Writeでイベントを発行する
        eventWriter.Write(new MyEvent());
    }
}
```

受信側のSystemでは`EventReader`を用いて発行されたイベントを読み取ります。

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial struct ReadEventSystem : ISystem
{
    // 取得したEventReaderはSystem内にキャッシュ
    EventReader<MyEvent> eventReader;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // GetEventReaderでEventReaderを取得
        eventReader = state.GetEventReader<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // eventReader.Read()で未読のイベントを読み取る
        foreach (var eventData in eventReader.Read())
        {
            Debug.Log("received!");
        }
    }
}
```

SystemがSystemBaseを継承したclassの場合は`this.GetEventWriter<MyEvent>()`または`this.GetEventWriter<MyEvent>()`でEventWriter/EventReaderの取得を行うことができます。

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial class WriteEventSystemClass : SystemBase
{
    EventWriter<MyEvent> eventWriter;

    [BurstCompile]
    protected override OnCreate()
    {
        // this.GetEventWriterでEventWriterを取得
        eventWriter = this.GetEventWriter<MyEvent>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        eventWriter.Write(new MyEvent());
    }
}
```

> **Warning**
> EventWriter/EventReaderは必ずOnCreateで取得してキャッシュを行なってください。特にEventReaderは各Readerごとに読み取ったイベントのカウントを記録するため、読み取りのたびに`state.GetEventReader()`を呼び出すとイベントが重複して読み取られる可能性があります。

## イベントの仕組み

Entities Eventsでは`RegisterEvent`属性で登録された型ごとに、イベントのバッファを保持するシングルトンなEntityとバッファの更新を行うSystemを生成します。生成されたEventSystemは`EventSystemGroup`内で実行され、フレームごとにイベントバッファをクリアします。

ただし、イベントは送信されたフレームの後に1フレームだけ余分に保持されます。そのためSystemは現在のフレームでイベントを読み取れなかった際に、次のフレームでイベントを読み取ることができます。

これにより送信/受信の順序に関わらずイベントを処理できることが保証されますが、受信側のSystemが送信側のSystemより先に実行される場合には1フレームの遅延が生じることに注意してください。これを防ぐには`UpdateBefore`属性や`UpdateAfter`属性を用いてSystem間の実行順を明示的に指定します。

またイベントの寿命は2フレームであるため、毎フレーム読み取りを行わない場合にはイベントが失われる可能性があることに注意してください。バッファの更新を手動で行いたい場合には、以下の`Events<T>`を用いて独自のEventSystemを作成できます。

## Events<T>

イベントの情報を保持するコレクションとして、独自のNativeContainer`Events<T>`が用意されています。

```cs
using Unity.Collections;
using EntitiesEvents;

// 新たなEventsを作成
var events = new Events<MyEvent>(32, Allocator.Temp);
```

作成したEventsは`Update`を呼び出して更新を行います。これによって内部のバッファがスワップされ、同時に最も古いバッファが削除されます。
イベントの蓄積によるメモリの消費を防ぐため、更新は毎フレーム行うことが推奨されます。

```cs
// Updateを呼び出してバッファのクリアとスワップを行う
events.Update();
```

書き込みや読み取りは`EventWriter/EventReader`を介して行います。これは`GetWriter/GetReader`で取得が可能です。

```cs
// EventWriterを取得して書き込み行う
var eventWriter = events.GetWriter();
eventWriter.Write(new MyEvent());

// EventReaderを取得して読み取りを行う
var eventReader = events.GetReader();
```

使用後は他のNativeContainerと同様に`Dispose`でメモリの解放を行う必要があります。これを忘れるとメモリリークを起こすので注意してください。

```cs
// Disposeでコンテナを破棄し、メモリの解放を行う
events.Dispose();
```

## ライセンス

[MIT License](LICENSE)

