# LoopMusicPlayer
## 概要
`LOOPSTART・[LOOPLENGTH,LOOPEND]`
のタグをもとにループ再生するミュージックプレーヤー  
ループタグがなかった場合、曲全体をループします

## 仕様
### 導入
* macOS  
  Applicationsフォルダに移動後、以下の作業をしてください
  - `xattr -rc /Applications/LoopMusicPlayer.app`

### 主な使用方法
* 音声ファイルをD&Dするか、「ファイルを開く」より選択してリストに追加してください。
* 再生したいファイルをリストから選択し、ダブルクリックしてください。

### 注意事項
* D&D時・ファイルの追加時には複数ファイルを選択することができます。
* ループ時に指定されたサンプルから数サンプルずれている可能性があります。(計算式がずさん)

### オプション
#### リピート方式
* **リピートしない**  
再生が終了しても、何もしません。

* **1曲リピート**  
再生が終了次第、同じ曲が最初から再生されます。

* **全曲リピート**  
再生が終了次第、プレイリスト上の次の曲が再生されます。

* **ランダムリピート**  
再生が終了次第、プレイリスト上の曲をランダムで選択し、再生されます。

#### ループ
ループタグを元にループするか

#### 更新間隔
音声バッファの更新確認間隔です。  
5ms - 100msを指定できます。

#### バッファ長
音声バッファの長さです。  
更新間隔 - 500msを指定できます。  
大きければ大きいほど再生は安定しますが、シーク時のラグが増えます。

#### 時間形式
プレーヤー画面での現在時間表示形式を指定します。

- **経過時間**  
  ループ回数を含めた再生開始からの経過時間を表示します。接頭辞に「+」がつきます。
- **シーク時間**  
  曲上の現在時刻を表示します。
- **残り時間**  
  曲の残り時間を表示します。接頭辞に「-」がつきます。

#### 常に手前に表示
ウィンドウが常に手前に表示されます。(Desktop版のみ有効)

## 開発環境(動作確認環境)
OS
* Windows 11 (Ver.23H2) (x64)
* macOS 14.5 (arm64)
* Ubuntu 22.04 LTS (x64, WSL)
* Android 12 (arm64)

Editor
* Visual Studio Community 2022
* Visual Studio Code

## 対応環境
* Windows 10+ (x86, x64)
* macOS 12+ (x64, arm64)
* Linux (arm32, arm64, x64) (X11, glibc)
* Android 8.0+ (armeabi-v7a, arm64-v8a)

Linuxの各種ディストロ対応については[こちら](https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md)をご覧ください

## 謝辞
各依存パッケージを作成していただいてる方々に感謝を申し上げます。

また、このソフトはぽかん氏のLooplayを参考に作成いたしました。ありがとうございます。

## 免責事項
このプログラムを使用し発生した、いかなる不具合・損失に対しても、一切の責任を負いません。(MITライセンス準拠)

## 作者より
これは元々私自身がmacOS・Linux環境で作業用BGMを聞くため、いい感じにループして聞ければいいや程度で作ったやつです。

PullRequest・Issue大歓迎です！

## 更新履歴
|バージョン |日付(JST) |                                       実装内容                                       |
|:---------:|:--------:|:-------------------------------------------------------------------------------------|
|Ver.1.0.0.0|2024-02-17|Avalonia UI化・Android対応・プレイリスト入れ替え対応・バッファサイズ変更対応          |
|Ver.1.0.0.1|2024-05-16|ライセンス文字列の自動折り返し                                                        |
|Ver.1.0.0.2|2024-06-27|曲移行時に自動再生されない場合がある問題の修正                                        |
|Ver.1.0.0.3|2024-08-19|設定ファイルが不正な状態である場合、起動できなくなる問題の修正                        |

<details><summary>旧版(GTK3版)</summary>

|バージョン |日付(JST) |                                       実装内容                                       |
|:---------:|:--------:|:-------------------------------------------------------------------------------------|
|Ver.0.1.0.0|2021-04-10|初版                                                                                  |
|Ver.0.1.0.1|2021-04-10|シークバーの見た目を修正                                                              |
|Ver.0.1.1.0|2021-04-11|サウンド再生時のバッファサイズを広げ、デバイス依存の不具合が起こりにくいよう修正      |
|Ver.0.2.0.0|2021-06-24|ループ方法の実装                                                                      |
|Ver.0.3.0.0|2021-06-29|オンメモリ再生の実装                                                                  |
|Ver.0.4.0.0|2021-07-03|デコード作業を全てBASSに任せるよう変更 (OGGファイル以外も音声のみは読み込めるように)  |
|Ver.0.4.1.0|2021-10-28|LinuxでWindowが非表示の際に、次の曲に移行できない問題の修正                           |
|Ver.0.4.2.0|2021-11-11|フレームワークを.NET 6に変更                                                          |
|Ver.0.4.2.1|2021-11-11|ストリーミング再生を用いた際に、正常にループできない可能性がある問題の修正(BASSの更新)|
|Ver.0.4.2.2|2021-11-17|プロジェクトの内部を変更したためのバージョン更新                                      |
|Ver.0.4.2.3|2021-11-17|音声ファイル読み込み時に落ちる問題の修正                                              |
|Ver.0.4.2.4|2021-11-21|音声ファイル読み込み時に数サンプル音声が再生されてしまう問題の修正                    |
|Ver.0.4.3.0|2021-12-22|デバイスオープン時の周波数をデバイスに合わせるように変更                              |
|Ver.0.4.3.1|2021-12-23|ストリーミング再生時に正常にループしない可能性がある問題の修正                        |
|Ver.0.4.4.0|2021-12-30|再生デバイス情報表示メニューの追加                                                    |
|Ver.0.5.0.0|2021-12-31|オンメモリ再生の削除                                                                  |
|Ver.0.6.0.0|2021-12-31|Opus・Flac・WavPackが格納されたファイルを読み込めるように                             |
|Ver.0.6.0.1|2022-01-01|「常に最前面に表示」を有効中にバージョン情報を表示した際、操作不能になる問題の修正    |
|Ver.0.6.0.2|2022-01-07|ループ時のサンプル数計算の修正(処理落ち時に次の曲に移行してしまう問題の修正)          |
|Ver.0.6.0.3|2022-01-08|頒布ファイルサイズの削減                                                              |
|Ver.0.6.1.0|2022-01-08|終了時の状態保存の実装                                                                |
|Ver.0.7.0.0|2022-01-16|言語拡張機能の実装                                                                    |
|Ver.0.7.0.1|2022-10-07|macOSでデバイスのサンプリングレートが自動的に上書きされる問題の修正                   |
|Ver.0.7.0.2|2024-01-07|osx-arm64に対応                                                                       |
|Ver.0.7.0.3|2024-01-07|フレームワークを.NET 8に変更                                                          |
</details>
