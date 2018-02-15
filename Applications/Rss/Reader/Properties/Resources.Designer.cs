﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cube.Net.App.Rss.Reader.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Cube.Net.App.Rss.Reader.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   厳密に型指定されたこのリソース クラスを使用して、すべての検索リソースに対し、
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   フィード取得中にエラーが発生しました。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string ErrorFeed {
            get {
                return ResourceManager.GetString("ErrorFeed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   URL は既に登録されています。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string ErrorFeedAlreadyExists {
            get {
                return ResourceManager.GetString("ErrorFeedAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   RSS または Atom フィードが見つかりません。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string ErrorFeedNotFound {
            get {
                return ResourceManager.GetString("ErrorFeedNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   OPML ファイル (*.opml, *.xml)|*.opml;*.OPML;*.xml;*.XML|すべてのファイル (*.*)|*.* に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string FilterOpml {
            get {
                return ResourceManager.GetString("FilterOpml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &lt;!DOCTYPE html&gt;
        ///&lt;html lang=&quot;ja&quot;&gt;
        ///
        ///&lt;head&gt;
        ///&lt;meta charset=&quot;utf-8&quot;&gt;
        ///&lt;meta name=&quot;author&quot; content=&quot;CubeSoft, Inc.&quot;&gt;
        ///
        ///&lt;title&gt;Loading...&lt;/title&gt;
        ///
        ///&lt;style&gt;
        ///.container {
        ///    position : absolute;
        ///    top      : 0;
        ///    left     : 0;
        ///    right    : 0;
        ///    bottom   : 0;
        ///}
        ///
        ///.box {
        ///    width    : 32px;
        ///    height   : 32px;
        ///    position : absolute;
        ///    top      : 50%;
        ///    left     : 50%;
        ///    margin   : -32px 0 0 -32px;
        ///}
        ///&lt;/style&gt;
        ///&lt;/head&gt;
        ///
        ///&lt;body&gt;
        ///&lt;div class=&quot;container&quot;&gt;
        ///&lt;div class=&quot;box&quot;&gt;
        ///&lt;img s [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Loading {
            get {
                return ResourceManager.GetString("Loading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   型 System.Drawing.Bitmap のローカライズされたリソースを検索します。
        /// </summary>
        internal static System.Drawing.Bitmap Logo {
            get {
                object obj = ResourceManager.GetObject("Logo", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   自動 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageAutoFrequency {
            get {
                return ResourceManager.GetString("MessageAutoFrequency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   ファイル名を指定して保存 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageExport {
            get {
                return ResourceManager.GetString("MessageExport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   高頻度 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageHighFrequency {
            get {
                return ResourceManager.GetString("MessageHighFrequency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   インポートするファイルを選択 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageImport {
            get {
                return ResourceManager.GetString("MessageImport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   購読中の Web サイトは全て削除され、インポートされた情報で上書きされます。エクスポート機能を利用するなどして、あらかじめバックアップを取得して下さい。
        ///
        ///インポートを実行しますか？ に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageImportWarning {
            get {
                return ResourceManager.GetString("MessageImportWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   最終チェック に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageLastChecked {
            get {
                return ResourceManager.GetString("MessageLastChecked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   低頻度 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageLowFrequency {
            get {
                return ResourceManager.GetString("MessageLowFrequency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   新しいフォルダー に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageNewCategory {
            get {
                return ResourceManager.GetString("MessageNewCategory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   チェックしない に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageNoneFrequency {
            get {
                return ResourceManager.GetString("MessageNoneFrequency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   新着記事はありません ({0}) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageNoReceived {
            get {
                return ResourceManager.GetString("MessageNoReceived", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0} 件の新着記事 ({1}) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageReceived {
            get {
                return ResourceManager.GetString("MessageReceived", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0} を削除しますか？
        ///カテゴリを指定した場合、カテゴリ中の Web サイトも全て削除されます。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MessageRemove {
            get {
                return ResourceManager.GetString("MessageRemove", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD HTML 4.01 Transitional//EN&quot; &quot;http://www.w3.org/TR/html4/loose.dtd&quot;&gt;
        ///&lt;html&gt;
        ///
        ///&lt;head&gt;
        ///&lt;meta http-equiv=&quot;Content-Type&quot; content=&quot;text/html; charset=UTF-8&quot;&gt;
        ///&lt;meta http-equiv=&quot;Content-Style-Type&quot; content=&quot;text/css&quot;&gt;
        ///&lt;meta http-equiv=&quot;Content-Script-Type&quot; content=&quot;text/javascript&quot;&gt;
        ///
        ///&lt;style type=&quot;text/css&quot;&gt;
        ///{0}
        ///&lt;/style&gt;
        ///
        ///&lt;base href=&quot;{1}&quot;&gt;
        ///&lt;/head&gt;
        ///
        ///&lt;body&gt;
        ///&lt;div class=&quot;cuberss-container&quot;&gt;
        ///&lt;div class=&quot;cuberss-title&quot;&gt;&lt;a href=&quot;{1}&quot; target=&quot;_blank&quot;&gt;{2}&lt;/a&gt;&lt;/div&gt;
        ///&lt;div cla [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Skeleton {
            get {
                return ResourceManager.GetString("Skeleton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   body {
        ///    margin          : 0;
        ///    padding         : 0;
        ///    border          : 0;
        ///    color           : #000;
        ///    font-size: 14px;
        ///    font-family     : &quot;メイリオ&quot;, &quot;Meiryo&quot;,
        ///    &quot;游ゴシック&quot;, &quot;Yu Gothic&quot;, &quot;游ゴシック体&quot;, &quot;YuGothic&quot;,
        ///    &quot;ＭＳ Ｐゴシック&quot;, &quot;MS PGothic&quot;,
        ///    sans-serif;
        ///}
        ///
        ///img {
        ///    max-width       : 100%;
        ///}
        ///
        ///a, a:visited {
        ///    color           : #00007f;
        ///}
        ///
        ///.cuberss-container {
        ///    margin          : 0 48px;
        ///}
        ///
        ///.cuberss-title {
        ///    margin          : 0;
        ///    padding         : 0;
        ///    font-s [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SkeletonStyle {
            get {
                return ResourceManager.GetString("SkeletonStyle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   確認 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string TitleInformation {
            get {
                return ResourceManager.GetString("TitleInformation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   注意 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string TitleWarning {
            get {
                return ResourceManager.GetString("TitleWarning", resourceCulture);
            }
        }
    }
}
