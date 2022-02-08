using System;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MobileConsole
{
    public static class ShareBridge
    {
        public static void ShareText(string text)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            TextEditor textEditor = new TextEditor
            {
                text = text
            };
            textEditor.SelectAll();
            textEditor.Copy();
            Debug.Log("Log has been coppied to Clipboard");
	        
            
#elif UNITY_IOS || UNITY_ANDROID
#if YASIRKULA_NATIVE_SHARE
            new NativeShare().SetText(text).Share();
#elif PIXEPTION_NATIVE_SHARE
            UnityNative.Sharing.UnityNativeSharing.Create().ShareText(text);            
#else
            Debug.LogError("No sharing package found, please install one of the supported options: \n" +
                           "Pixeption Native Share: https://github.com/NicholasSheehan/Unity-Native-Sharing" +
                           "\n or \n" +
                           "Yasirkula Native Share: https://github.com/yasirkula/UnityNativeShare");

#endif
#endif
        }

        public static void ShareAllLogs(string logs)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
	        if (LogConsoleSettings.Instance.useShareLogViaMail)
	        {
		        ShareLogToEmailClient(logs);
	        }
	        else
	        {
		        Debug.LogError("Share by email is not enabled");
	        }
#elif UNITY_IOS || UNITY_ANDROID
			// Android intent limit size is 1MB (https://stackoverflow.com/questions/39098590)
			// Thus we save it as a file then share the file instead
			string fileName = string.Format("{0}_{1}({2})_{3}.log",
				Application.productName,
				Application.version,
				EventBridge.AppVersionCode,
				DateTime.Now.ToString("MMMM-dd_HH-mm-ss-ff"));
			string folderPath = Path.Combine(Application.persistentDataPath, "MCK_Logs");
			string filePath = Path.Combine(folderPath, fileName);

			// Make sure the directory is created
			Directory.CreateDirectory(folderPath);
			File.WriteAllText(filePath, logs);
#if YASIRKULA_NATIVE_SHARE
			new NativeShare().AddFile(filePath).Share();
#elif PIXEPTION_NATIVE_SHARE
		        UnityNative.Sharing.UnityNativeSharing.Create().ShareScreenshotAndText("", filePath, true,
			        "Select App To Share With", "text/plain");

#else
	        Debug.LogError("No sharing package found, please install one of the supported options: \n" +
	                       "Pixeption Native Share: https://github.com/NicholasSheehan/Unity-Native-Sharing" +
	                       "\n or \n" +
	                       "Yasirkula Native Share: https://github.com/yasirkula/UnityNativeShare");   
#endif
#endif
        }
        
        public static void ShareFiles(params string[] filePaths)
        {
	        if (filePaths == null || filePaths.Length == 0)
		        return;

#if YASIRKULA_NATIVE_SHARE
	        NativeShare filesShare = new NativeShare();
	        for (int i = 0; i < filePaths.Length; i++)
	        {
		        filesShare.AddFile(filePaths[i]);
	        }

	        filesShare.Share();
#elif PIXEPTION_NATIVE_SHARE
	        UnityNative.Sharing.IUnityNativeSharing unityNativeSharing = UnityNative.Sharing.UnityNativeSharing.Create();

	        for (int i = 0; i < filePaths.Length; i++)
	        {
		        unityNativeSharing.ShareScreenshotAndText("", filePaths[i], true,
			        "Select App To Share With", "text/plain"); 
	        }
#endif
        }
        
        static void ShareLogToEmailClient(string logs)
        {
            string recipients = string.Empty;
            if (LogConsoleSettings.Instance.mailRecipients != null)
            {
                recipients = string.Join(",", LogConsoleSettings.Instance.mailRecipients);
            }

            string fullUrl = string.Format(LogConsoleSettings.mailTemplate,
                EscapeUrl(recipients),
                EscapeUrl(LogConsoleSettings.Instance.mailSubject),
                EscapeUrl(logs));
			
            Application.OpenURL(fullUrl);
        }

        static string EscapeUrl(string content)
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEngine.Networking.UnityWebRequest.EscapeURL(content).Replace("+", "%20");
#else
			return WWW.EscapeURL(content).Replace("+", "%20");
#endif		
        }

    }
}
