using System.Runtime.InteropServices;
using UnityEngine;

public static class JavaScriptUtils
{
    [DllImport("__Internal")]
    private static extern void CopyToClipboardImpl(string text);
    public static void CopyStringTOClipboard(string textToCopy)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CopyToClipboardImpl(textToCopy);
#else
        GUIUtility.systemCopyBuffer = textToCopy;
#endif

        Debug.Log($"Copied: {textToCopy}");
    }

    [DllImport("__Internal")] 
    private static extern string PoseidonHash(string[] arrayOfElements, int length);
    public static string PoseidonHashHelper(string[] arrayOfElements)
    {
        return PoseidonHash(arrayOfElements, arrayOfElements.Length);
    }


    [DllImport("__Internal")]
    public static extern void RefreshPage();

    [DllImport("__Internal")]
    public static extern void OpenURL(string url);

    [DllImport("__Internal")]
    public static extern void SaveToLocalStorage(string key, string value);

    [DllImport("__Internal")]
    public static extern string getAllData();

    [DllImport("__Internal")]
    public static extern void ClearAllLocalData();

}
