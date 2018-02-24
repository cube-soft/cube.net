CubeRSS Reader
====

Copyright (c) 2010 CubeSoft Inc.

support@cube-soft.jp  
http://www.cube-soft.jp/

## はじめに

CubeRSS Reader は、RSS や Atom と呼ばれる情報を利用する事によって、確認したい
サイトの URL を登録するだけで自動的に新着情報を取得し、ユーザにお知らせします。
カテゴリ機能や細かな挙動のカスタマイズなども可能で、とても便利な情報収集ツールと
なっています。

CubeRSS Reader を使用するためには、Microsoft .NET Framework 3.5 以上が
インストールされている必要があります（4.5.2 以上を推奨）。
.NET Framework は、以下のURL からダウンロードして下さい。

* Microsoft .NET Framework 3.5  
  http://www.microsoft.com/ja-jp/download/details.aspx?id=22
* Microsoft .NET Framework 4.5.2  
  https://www.microsoft.com/ja-JP/download/details.aspx?id=42643

## 利用方法

新たな Web サイトを登録する場合、左下の “+”（プラス）ボタンをクリックして
表示される画面で Web サイト自体の URL を入力して追加ボタンをクリックする事で、
CubeRSS Reader が自動的に RSS/Atom フィードの URL を検索して登録します。
ただし、Web サイトによっては見つけられない事がありますので、その場合は
RSS/Atom フィード URL 自体を入力して追加して下さい。

CubeRSS Reader 右上のボタンから「設定」をクリックする事で設定画面が表示されます。
設定可能な項目は以下の通りです。

* チェック間隔  
  CubeRSS Reader は、登録されている Web サイトの更新頻度に応じて、新着情報を確認する
  頻度を 2 種類に分類しています。初期設定では、更新頻度の高い Web サイトに対しては
  1 時間毎、低いサイトに対しては 24 時間毎となっています。
  チェック頻度を変更したい場合は、これらの値を変更して下さい。
* 軽量モード  
  有効にすると、利用する PC によっては動作が重くなりそうな処理を簡略化します。
* 新しいウィンドウを有効にする  
  有効の場合、リンクを右クリックして「新しいウィンドウで開く」を選択した場合や、
  about=”_blank” 指定のあるリンクをクリックした時に既定のブラウザでリンク先を表示します。
  無効の場合は CubeRSS Reader 上で新しいウィンドウを生成せずに遷移します。
* 記事の取得状況をステータスバーに表示する  
  CubeRSS Reader は登録された Web サイトを定期的にチェックしていますが、
  この項目が有効の場合、チェックした結果をステータスバーに表示します。
* ソフトウェアのアップデートを確認する  
  有効の場合、CubeRSS Reader のアップデート状況を定期的に確認し、バージョンアップの
  あった場合はその旨が通知されます。

## 利用ライブラリ

CubeRSS Reader は、以下のライブラリを利用しています。
それぞれのライブラリについては、記載した URL から取得することができます。

* AlphaFS
    - MIT License
    - http://alphafs.alphaleonis.com/
    - https://www.nuget.org/packages/AlphaFS/
* AsyncBridge
    - MIT License
    - http://omermor.github.io/AsyncBridge/
    - https://www.nuget.org/packages/AsyncBridge.Net35
* gong-wpf-dragdrop
    - BSD 3-Clause License
    - https://github.com/punker76/gong-wpf-dragdrop
    - https://www.nuget.org/packages/gong-wpf-dragdrop/
* log4net
    - Apache License, Version 2.0
    - http://logging.apache.org/log4net/
    - https://www.nuget.org/packages/log4net/
* MVVM Light Toolkit
    - MIT License
    - http://www.galasoft.ch/mvvm
    - https://www.nuget.org/packages/MvvmLight/

## バージョン履歴

* 2018/02/23 version 0.1.0β
    - 最初の公開バージョン
