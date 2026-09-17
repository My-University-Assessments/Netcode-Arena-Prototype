using ArenaPrototype.Util;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityNetworkHelper))]
public class UnityNetworkHelperInspector : Editor
{
    int buttonHeight = 23;

    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Network Controls", EditorStyles.boldLabel);

        // Grey out controls if not playing
        EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);

        // Check network state
        bool isListening = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        bool isClient = isListening && !isHost;

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawStatusIndicator("Listening", isListening);
            DrawStatusIndicator("Is Host", isHost);
        }

        // Host Controls
        string hostButtonLabel = isHost ? "Stop Host" : "Start Host";
        if (GUILayout.Button(hostButtonLabel, GUILayout.Height(buttonHeight)))
        {
            if (isHost)
                NetworkEventManager.OnRequestStopUnityHost?.Invoke();
            else
                NetworkEventManager.OnRequestStartUnityHost?.Invoke();
        }

        // Client Controls
        string clientButtonLabel = isClient ? "Stop Client" : "Start Client";
        if (GUILayout.Button(clientButtonLabel, GUILayout.Height(buttonHeight)))
        {
            if (isClient)
                NetworkEventManager.OnRequestStopUnityClient?.Invoke();
            else
                NetworkEventManager.OnRequestStartUnityClient?.Invoke();
        }

        EditorGUI.EndDisabledGroup();

    }

    private void DrawStatusIndicator(string label, bool isActive)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = isActive ? Color.green : Color.red }
        };

        string status = isActive ? "✓ Yes" : "✗ No";
        EditorGUILayout.LabelField($"{label}: {status}", style);
    }
}