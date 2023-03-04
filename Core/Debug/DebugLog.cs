using System;

namespace Light_Up_Your_Life.Core;

/// <summary>
/// デバッグ用のコンソールログクラス
/// </summary>
public class DebugLog
{
	//初期イベントをアタッチするために最初にnewする
	private static DebugLog debug = new DebugLog();

	//MainLog以外にイベントを追加して、ログが出力された後に独自の処理を行うことも可能
	private delegate void LogEventHandler(DebugLogProperties properties);
	private delegate void ErrorLogEventHandler(DebugLogProperties properties, Exception exception);
	private static event LogEventHandler SystemLogEvent;
	private static event LogEventHandler WarnLogEvent;
	private static event ErrorLogEventHandler ErrorLogEvent;

	//TODO: イベントに外部からデリゲートを追加できるようにパブリックの追加用関数を作る

	//初期イベントをアタッチする
	private DebugLog()
	{
		//システムログ
		SystemLogEvent += ((properties) =>
		{
			Console.WriteLine("[" + properties.time + "]" + "[SYSTEM] : " + properties.log.ToString());
		});

		//警告ログ
		WarnLogEvent += ((properties) =>
		{
			Console.WriteLine("[" + properties.time + "]" + "[WARNING] : " + properties.log.ToString());
		});

		//エラーログ
		ErrorLogEvent += ((properties, exception) =>
		{
			Console.WriteLine("[" + properties.time + "]" + "[ERROR] : " + properties.log.ToString() + "\n" + exception.StackTrace);
		});
	}

	//各ログの関数ごとに対応するイベントを呼び出す
	public static void SystemLog(object log)
	{
		var properties = new DebugLogProperties(log, DateTime.Now);
		SystemLogEvent.Invoke(properties);
	}

	public static void WarnLog(object log)
	{
		var properties = new DebugLogProperties(log, DateTime.Now);
		WarnLogEvent.Invoke(properties);
	}

	public static void ErrorLog(object log, Exception exception)
	{
		var properties = new DebugLogProperties(log, DateTime.Now);
		ErrorLogEvent.Invoke(properties, exception);
	}
	///

	/// <summary>
	/// デバッグログ用のプロパティクラス。
	/// このクラスの中にログに必要なプロパティが保管されている
	/// </summary>
	private class DebugLogProperties
	{
		public readonly object log;
		public readonly DateTime time;

		public DebugLogProperties(object log, DateTime time)
		{
			this.log = log;
			this.time = time;
		}
	}
}
