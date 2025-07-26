using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SeatingChartApp.Runtime.UI;
using SeatingChartApp.Runtime.Systems;

namespace SeatingChartApp.Editor
{
    public class UIAutoSetupTool
    {
        [MenuItem("Tools/SeatingChartApp/Auto-Generate Full UI")]
        public static void GenerateUI()
        {
            // Root Canvas
            GameObject canvasGO = new GameObject("SeatingChart_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.layer = LayerMask.NameToLayer("UI");

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2048, 1536);

            // EventSystem (required)
            if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            // Panels
            GameObject assignmentPanel = CreatePanel("AssignmentPanel", canvasGO.transform, new Vector2(400, 600));
            GameObject loginPanel = CreatePanel("LoginPanel", canvasGO.transform, new Vector2(300, 200));
            GameObject adminPanel = CreatePanel("AdminToolsPanel", canvasGO.transform, new Vector2(500, 400));
            GameObject topBar = CreatePanel("TopBar", canvasGO.transform, new Vector2(2048, 100));
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0, 1);
            topBarRect.anchorMax = new Vector2(1, 1);
            topBarRect.pivot = new Vector2(0.5f, 1);
            topBarRect.anchoredPosition = new Vector2(0, 0);

            // SeatingUIManager setup
            SeatingUIManager seatingUI = canvasGO.AddComponent<SeatingUIManager>();
            seatingUI.assignmentPanel = assignmentPanel;

            TMP_InputField firstName = CreateInputField("FirstNameInput", assignmentPanel.transform, "First Name");
            TMP_InputField lastName = CreateInputField("LastNameInput", assignmentPanel.transform, "Last Name");
            TMP_InputField roomNumber = CreateInputField("RoomNumberInput", assignmentPanel.transform, "Room");
            TMP_InputField partySize = CreateInputField("PartySizeInput", assignmentPanel.transform, "Party Size");

            seatingUI.firstNameInput = firstName;
            seatingUI.lastNameInput = lastName;
            seatingUI.roomNumberInput = roomNumber;
            seatingUI.partySizeInput = partySize;

            seatingUI.assignButton = CreateButton("AssignButton", assignmentPanel.transform, "Assign");
            seatingUI.clearButton = CreateButton("ClearButton", assignmentPanel.transform, "Clear");
            seatingUI.assignPreviousButton = CreateButton("AssignPreviousButton", assignmentPanel.transform, "Assign Previous");
            seatingUI.toggleOutOfServiceButton = CreateButton("ToggleOutOfServiceButton", assignmentPanel.transform, "Toggle OOS");

            seatingUI.errorText = CreateTMPText("ErrorText", assignmentPanel.transform, "Error will appear here");

            // LoginUIManager setup
            LoginUIManager loginUI = canvasGO.AddComponent<LoginUIManager>();
            loginUI.loginPanel = loginPanel;
            loginUI.passwordInput = CreateInputField("PasswordInput", loginPanel.transform, "Password");
            loginUI.loginButton = CreateButton("LoginButton", loginPanel.transform, "Login");
            loginUI.logoutButton = CreateButton("LogoutButton", loginPanel.transform, "Logout");

            // AdminToolsManager setup
            AdminToolsManager adminTools = canvasGO.AddComponent<AdminToolsManager>();
            adminTools.adminToolsPanel = adminPanel;
            adminTools.clearAllButton = CreateButton("ClearAllButton", adminPanel.transform, "Clear All");
            adminTools.bulkStateDropdown = CreateDropdown("BulkStateDropdown", adminPanel.transform, new string[] { "Available", "Occupied", "OutOfService", "Cleaning", "Reserved" });
            adminTools.searchInput = CreateInputField("SearchInput", adminPanel.transform, "Search Name/Room");
            adminTools.searchButton = CreateButton("SearchButton", adminPanel.transform, "Search");
            adminTools.filterDropdown = CreateDropdown("FilterDropdown", adminPanel.transform, new string[] { "All", "Available", "Occupied", "OutOfService", "Cleaning", "Reserved" });
            adminTools.exportReportButton = CreateButton("ExportReportButton", adminPanel.transform, "Export CSV");

            // TopBar button to open Login Panel
            Button loginOpenButton = CreateButton("OpenLoginButton", topBar.transform, "Admin Login");
            loginOpenButton.onClick.AddListener(() =>
            {
                loginUI.ShowLoginPanel();
            });

            Selection.activeGameObject = canvasGO;
        }

        // Utilities
        private static GameObject CreatePanel(string name, Transform parent, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;
            return go;
        }

        private static TMP_InputField CreateInputField(string name, Transform parent, string placeholder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            TMP_InputField input = go.AddComponent<TMP_InputField>();
            GameObject textGO = new GameObject("Text", typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform);
            input.textComponent = textGO.GetComponent<TextMeshProUGUI>();
            GameObject placeholderGO = new GameObject("Placeholder", typeof(TextMeshProUGUI));
            placeholderGO.transform.SetParent(go.transform);
            placeholderGO.GetComponent<TextMeshProUGUI>().text = placeholder;
            input.placeholder = placeholderGO.GetComponent<TextMeshProUGUI>();
            return input;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            GameObject go = new GameObject(name, typeof(Button), typeof(Image));
            go.transform.SetParent(parent);
            Button button = go.GetComponent<Button>();
            GameObject textGO = new GameObject("Text", typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform);
            textGO.GetComponent<TextMeshProUGUI>().text = label;
            return button;
        }

        private static TMP_Dropdown CreateDropdown(string name, Transform parent, string[] options)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            TMP_Dropdown dropdown = go.AddComponent<TMP_Dropdown>();
            dropdown.options.Clear();
            foreach (var option in options)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(option));
            }
            return dropdown;
        }

        private static TMP_Text CreateTMPText(string name, Transform parent, string content)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            TMP_Text text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            return text;
        }
    }
}
