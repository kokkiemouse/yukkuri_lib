﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace yukkuri_lib_interface
{
    public delegate SPEAK_RETURN SpeakDelegate(yukkuri_lib_interface_EventArgs eventargs);
    public delegate void CloseDelegate();
    public delegate void Dll_load_delegate(yukkuri_lib_interface_dllload_args dllargs);
    public delegate void init_delegate();
    public delegate void dll_loaded_delegate();
    public delegate void Discard_loop();
    /// <summary>
    /// Server(64bit側)からのCallのインターフェース
    /// <see cref="MarshalByRefObject"/>を継承してる。
    /// </summary>
    public class EventCallbackSink : MarshalByRefObject
    {
        /// <summary>
        /// Serverから<see cref="yukkuri_lib_interface.Speak_to_client(yukkuri_lib_interface_EventClass)"/>を呼ぶと呼ばれるイベント。
        /// </summary>
        public event SpeakDelegate OnSpeak;
        /// <summary>
        /// Serverから<see cref="DllLoadtoClient(yukkuri_lib_interface_dllload_args)"/>を呼ぶと呼ばれるイベント。
        /// </summary>
        public event Dll_load_delegate OnDllLoad;
        /// <summary>
        /// Serverから<see cref="Close_toClient"/>を呼ぶと呼ばれるイベント。
        /// </summary>
        public event CloseDelegate OnClose;
        /// <summary>
        /// <see cref="EventCallbackSink"/>のコンストラクタ。何もしないよ。
        /// </summary>
        public EventCallbackSink()
        {

        }
        /// <summary>
        /// Serverから呼ばれるやつ。
        /// パラメータに指定したものでwavを生成するよ。
        /// </summary>
        /// <param name="evargs">パラメータのオブジェクト</param>
        /// <returns>wavファイルの<see cref="byte"/>配列</returns>
        public SPEAK_RETURN SpeakCallBackToClient(yukkuri_lib_interface_EventArgs evargs)
        {
            return OnSpeak?.Invoke(evargs); //OnSpeakイベントを呼び出し。
        }
        /// <summary>
        /// Serverから呼ばれるやつ。
        /// Dllをロードする際に呼ばれる。
        /// </summary>
        /// <param name="dargs">DLLファイルに関するオブジェクト</param>
        public void DllLoadtoClient(yukkuri_lib_interface_dllload_args dargs)
        {
            try
            {
                OnDllLoad?.Invoke(dargs);   //OnDllLoadイベントを呼び出し。
            }catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Serverから終了する際に呼ばれるやつ。
        /// これを呼ぶと32bitの方が終了する。
        /// </summary>
        public void Close_toClient()
        {
            OnClose?.Invoke();  //OnCloseイベントを呼び出し。
        }

    }
    /// <summary>
    /// <see cref="yukkuri_lib_interface_dllload_args"/>で使われるやつ。
    /// スピード、テキストデータが入るよ。
    /// </summary>
    public class yukkuri_lib_interface_EventClass
    {
        /// <summary>
        ///音声記号列 
        /// </summary>
        public string textdata { get; set; }
        /// <summary>
        /// スピード(標準は100)
        /// </summary>
        public int speed { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="txtdata">音声記号列</param>
        /// <param name="speedkun">スピード(標準は100)</param>
        public yukkuri_lib_interface_EventClass(string txtdata, int speedkun)
        {
            speed = speedkun;
            textdata = txtdata; //突っ込んでるだけ。
        }
        /// <summary>
        /// 無いとエラーが起きた。
        /// 深い意味はない。
        /// </summary>
        public yukkuri_lib_interface_EventClass()
        {
            textdata = "";
            speed = 100;
        }
    }
    /// <summary>
    /// 通信用。
    /// <see cref="yukkuri_lib_interface.DllLoad_to_client(string)"/>で使われるよ。
    /// 
    /// </summary>
    [Serializable]
    public class yukkuri_lib_interface_dllload_args : ISerializable
    {
        /// <summary>
        /// dllのパス。
        /// 
        /// </summary>
        public string dll_path;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dllpath">dllのパス。</param>
        public yukkuri_lib_interface_dllload_args(string dllpath)
        {
            dll_path = dllpath;
        }
        /// <summary>
        /// カスタムデシリアライズ用。
        /// </summary>
        /// <param name="info">データが入るらしい。</param>
        /// <param name="context">使わん。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected yukkuri_lib_interface_dllload_args(SerializationInfo info,StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = info.GetString("dll_path");  //シリアライズされたJSONデータを取得。
            dll_path = ser.Deserialize<string>(json);   //jsonデータをデシリアライズして突っ込む。
        }
        /// <summary>
        /// シリアライズ用。
        /// </summary>
        /// <param name="info">シリアライズ用のデータ。</param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info,StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();  //シリアライザーを作成。
            var json = ser.Serialize(dll_path); //dll_pathをシリアライズ。
            info.AddValue("dll_path", json);    //シリアライズしたものを突っ込む。
        }
    }
    /// <summary>
    /// 通信用。
    /// <see cref="yukkuri_lib_interface.Speak_to_client(yukkuri_lib_interface_EventClass)"/>で使われる。
    /// </summary>
    [Serializable]
    public class yukkuri_lib_interface_EventArgs : ISerializable
    {
        /// <summary>
        /// データが入ってるオブジェクト。
        /// </summary>
        public yukkuri_lib_interface_EventClass eventargs;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="evebtar">データが入った<see cref="yukkuri_lib_interface_EventClass"/>オブジェクト</param>
        public yukkuri_lib_interface_EventArgs(yukkuri_lib_interface_EventClass evebtar)
        {
            this.eventargs = evebtar;
        }
        /// <summary>
        /// デシリアライズ用。
        /// </summary>
        /// <param name="info">データ処理先。</param>
        /// <param name="context">使わない。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected yukkuri_lib_interface_EventArgs(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = info.GetString("eventargs");
            eventargs = ser.Deserialize<yukkuri_lib_interface_EventClass>(json);
        }
        /// <summary>
        /// シリアライズ用
        /// </summary>
        /// <param name="info">データ</param>
        /// <param name="context">使わん。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = ser.Serialize(eventargs);
            info.AddValue("eventargs", json);
        }
    }
    /// <summary>
    /// Speakの戻り値
    /// </summary>
    [Serializable]
    public class SPEAK_RETURN
    {
        /// <summary>
        /// エラーに関する構造体。
        /// </summary>
        public DLL_LOAD_ERROR error;
        /// <summary>
        /// wavファイルの中身。
        /// </summary>
        public byte[] wavdata;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SPEAK_RETURN()
        {
            error = new DLL_LOAD_ERROR();
            wavdata = new byte[] { 0 };
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_error"> エラー用オブジェ</param>
        /// <param name="wavdata">データが入った<see cref="byte"/>オブジェクト</param>
        public SPEAK_RETURN(DLL_LOAD_ERROR _error, byte[] wavdata )
        {
            this.error =_error;
        }
        /// <summary>
        /// デシリアライズ用。
        /// </summary>
        /// <param name="info">データ処理先。</param>
        /// <param name="context">使わない。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected SPEAK_RETURN(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json_e = info.GetString("error");
            error = ser.Deserialize<DLL_LOAD_ERROR>(json_e);
            var json_b = info.GetString("wavdata");
            wavdata = ser.Deserialize<byte[]>(json_b);
        }
        /// <summary>
        /// シリアライズ用
        /// </summary>
        /// <param name="info">データ</param>
        /// <param name="context">使わん。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json_e = ser.Serialize(error);
            var json_b = ser.Serialize(wavdata);
            info.AddValue("error", json_e);
            info.AddValue("wavdata", json_b);
        }
    }
    /// <summary>
    /// DLLのロードエラー用
    /// </summary>
    [Serializable]
    public class DLL_LOAD_ERROR
    {
        /// <summary>
        /// エラーコード
        /// </summary>
        public DLL_ERR_CODE err_code;
        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string message;
        /// <summary>
        ///コンストラクタ
        /// </summary>
        public DLL_LOAD_ERROR()
        {
            err_code = DLL_ERR_CODE.NO_ERROR;
            message = "success";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="err_c">エラーコード</param>
        /// <param name="message">エラーメッセージ</param>
        public DLL_LOAD_ERROR(DLL_ERR_CODE err_c,string message)
        {
            this.err_code = err_c;
            this.message = message;
        }
        /// <summary>
        /// デシリアライズ用。
        /// </summary>
        /// <param name="info">データ処理先。</param>
        /// <param name="context">使わない。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected DLL_LOAD_ERROR(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json_e = info.GetString("err_code");
            var json_m = info.GetString("message");
            err_code = ser.Deserialize<DLL_ERR_CODE>(json_e);
            message = ser.Deserialize<string>(json_m);
        }
        /// <summary>
        /// シリアライズ用
        /// </summary>
        /// <param name="info">データ</param>
        /// <param name="context">使わん。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json_e = ser.Serialize(err_code);
            var json_m = ser.Serialize(message);
            info.AddValue("err_code", json_e);
            info.AddValue("message", json_m);
        }

    }
    /// <summary>
    /// <see cref="dll_loaded_delegate"/>のエラーコード
    /// </summary>
    public enum DLL_ERR_CODE
    {
        /// <summary>
        /// エラーなし
        /// </summary>
        NO_ERROR,
        /// <summary>
        /// IOエラー
        /// </summary>
        IO_ERROR,
        /// <summary>
        /// その他のエラー
        /// </summary>
        OTHER_ERROR,
        /// <summary>
        /// よくわからないぬるぽ
        /// </summary>
        NULLPOINTER_OTHER,
        /// <summary>
        /// メモリー不足
        /// </summary>
        out_of_memory,
        /// <summary>
        /// 定義されていない音声記号
        /// </summary>
        undefined_symbol,
        /// <summary>
        /// speedに負の数が指定された
        /// </summary>
        minus_speed,
        /// <summary>
        /// 未定義の区切り記号が検出。
        /// </summary>
        Undefined_delimiter_code_detection,

        /// <summary>
        /// タグの構文が丘people
        /// </summary>
        syntax_tag_error,
        /// <summary>
        /// タグが長い、あるいは終端記号 > が見つからない。
        /// </summary>
        tag_end_error,
        /// <summary>
        /// タグの値が丘people
        /// </summary>
        tag_value_invalid,
        /// <summary>
        /// 話す音声記号が見つからない
        /// </summary>
        text_not_found,
        /// <summary>
        /// 音声記号列が長すぎる
        /// </summary>
        too_long_text,
        /// <summary>
        /// 記号が多すぎる
        /// </summary>
        too_many_symbol,
        /// <summary>
        /// 音声記号列が長すぎる(内部バッファーオーバーー)
        /// </summary>
        too_long_text_buffer_over,
        /// <summary>
        /// ヒープメモリが不足
        /// </summary>
        out_of_heap_memory
    }
    /// <summary>
    /// 32bitと64bitをつなぐインターフェース。
    /// <see cref="MarshalByRefObject"/>を継承してる。
    /// </summary>
    public class yukkuri_lib_interface : MarshalByRefObject
    {
        private List<SpeakDelegate> eventListeners_speak = new List<SpeakDelegate>();
        private List<CloseDelegate> closeListeners = new List<CloseDelegate>();
        private List<Dll_load_delegate> eventListeners_dllload = new List<Dll_load_delegate>();
        /// <summary>
        /// 初期化完了後にクライアント(32bit)が呼び出すイベント。
        /// </summary>
        public event init_delegate Oninit;
        /// <summary>
        /// クライアントが破棄防止のため定期的に呼び出すやつ。
        /// </summary>
        public event Discard_loop Ondiscardloop;

        /// <summary>
        /// DLLが読み込まれた後にClient(32bit)が呼び出すイベント。
        /// </summary>
        public event dll_loaded_delegate OnDllLoaded;
        /// <summary>
        /// Clientが呼び出す。
        /// <see cref="yukkuri_lib_interface.Oninit"/>を呼び出すだけ。
        /// </summary>
        public void inited()
        {
            Oninit?.Invoke();
        }
        /// <summary>
        /// ClientがDiscard防止のため呼び出す。
        /// </summary>
        public void discardkun()
        {
            if(Ondiscardloop != null)
            {
                Ondiscardloop();
            }
            else
            {
                Debug.WriteLine("sex");
            }
        }
        /// <summary>
        /// Clientが呼び出す。
        /// <see cref="OnDllLoaded"/>を呼び出す。
        /// </summary>
        public void dll_loaded()
        {
            OnDllLoaded?.Invoke();
        }
        /// <summary>
        /// <see cref="Speak_to_client"/>のListenerに追加。
        /// Client側が使う。
        /// </summary>
        /// <param name="listener"><see cref="SpeakDelegate"/>型の関数。ラムダ式を使うと楽。</param>
        public void AddEventListener_Speak(SpeakDelegate listener)
        {
            eventListeners_speak.Add(listener);
        }

        /// <summary>
        /// <see cref="DllLoad_to_client"/>のListenerに追加。
        /// Client側が使う。
        /// </summary>
        /// <param name="listener"><see cref="Dll_load_delegate"/>型の関数。ラムダ式を使うと楽。</param>
        public void AddEventListener_Dllload(Dll_load_delegate listener)
        {
            eventListeners_dllload.Add(listener);
        }

        /// <summary>
        /// <see cref="Close_to_client"/>のListenerに追加。
        /// Client側が使う。
        /// </summary>
        /// <param name="cl"><see cref="CloseDelegate"/>型の関数。ラムダ式を使うと楽。</param>
        public void AddEventListener_close(CloseDelegate cl)
        {
            closeListeners.Add(cl);
        }
        /// <summary>
        /// Serverが呼び出す。
        /// 指定した内容でwavを生成して返す。
        /// </summary>
        /// <param name="paramkun">指定する内容。</param>
        /// <returns>wavファイル</returns>
        public SPEAK_RETURN Speak_to_client(yukkuri_lib_interface_EventClass paramkun)
        {
            yukkuri_lib_interface_EventArgs evt = new yukkuri_lib_interface_EventArgs(paramkun);    //引数を生成
            foreach(SpeakDelegate listener in eventListeners_speak) 
            {
                return listener(evt);   //実行する。
            }
            SPEAK_RETURN spr = new SPEAK_RETURN();
            spr.error.err_code = DLL_ERR_CODE.OTHER_ERROR;
            spr.error.message = "Event listener error";
            return spr;
        }
        /// <summary>
        /// Serverが使用。
        /// Clientを落とすときに使用。
        /// </summary>
        public void Close_to_client()
        {
            /*yukkuri_lib_close_args evt = new yukkuri_lib_close_args(cle);
            foreach(CloseDelegate clel in closeListeners)
            {
                cle=clel(evt);
            }
            */
            foreach(CloseDelegate clel in closeListeners)
            {
                clel(); //実行
            }
        }
        /// <summary>
        /// Serverが使用。
        /// Dllをロードさせるときに使用。
        /// </summary>
        /// <param name="dllpath"></param>
        public void DllLoad_to_client(string dllpath)
        {
            yukkuri_lib_interface_dllload_args evt = new yukkuri_lib_interface_dllload_args(dllpath);   //イベントの引数を作成。
            foreach (Dll_load_delegate listener in eventListeners_dllload)
            {
                try
                {
                    listener(evt);//実行
                }catch (Exception)
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 勝手に消されないように。
        /// </summary>
        /// <returns>しらん。</returns>
        /// 
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

    }
}
