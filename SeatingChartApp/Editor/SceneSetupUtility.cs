using SeatingChartApp.Runtime.Systems;
using SeatingChartApp.Runtime.UI;
using UnityEditor;
using UnityEngine;

namespace SeatingChartApp.Editor
{
    /// <summary>
    /// Provides editor utilities to help set up a scene with all the required
    /// managers and default objects for the SeatingChartApp.  Exposes a menu
    /// item under Tools/SeatingChartApp for convenience.  Requires the
    /// presence of prefabs in Resources/Prefabs for seats and UI panels.
    /// </summary>
    public static class SceneSetupUtility
    {
        [MenuItem("Tools/SeatingChartApp/Setup Scene")]
        public static void SetupScene()
        {
            // Ensure there is a LayoutManager
            if (Object.FindObjectOfType<LayoutManager>() == null)
            {
                new GameObject("LayoutManager").AddComponent<LayoutManager>();
            }
            // Ensure there is a UserRoleManager
            if (Object.FindObjectOfType<UserRoleManager>() == null)
            {
                new GameObject("UserRoleManager").AddComponent<UserRoleManager>();
            }
            // Ensure there is a SeatingUIManager
            if (Object.FindObjectOfType<SeatingUIManager>() == null)
            {
                GameObject uiGO = new GameObject("SeatingUIManager");
                uiGO.AddComponent<SeatingUIManager>();
            }
            // Ensure there is a LoginUIManager
            if (Object.FindObjectOfType<LoginUIManager>() == null)
            {
                GameObject loginGO = new GameObject("LoginUIManager");
                loginGO.AddComponent<LoginUIManager>();
            }
            // Ensure there is an AdminToolsManager
            if (Object.FindObjectOfType<AdminToolsManager>() == null)
            {
                GameObject toolsGO = new GameObject("AdminToolsManager");
                toolsGO.AddComponent<AdminToolsManager>();
            }
            // Add a SceneSetupValidator to catch missing components at runtime
            if (Object.FindObjectOfType<SceneSetupValidator>() == null)
            {
                new GameObject("SceneSetupValidator").AddComponent<SceneSetupValidator>();
            }
            EditorUtility.DisplayDialog("Scene Setup Complete", "The scene has been populated with the core SeatingChartApp components.  You still need to assign UI references in the Inspector and populate seats.", "OK");
        }
    }
}