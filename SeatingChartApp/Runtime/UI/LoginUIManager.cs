using SeatingChartApp.Runtime.Systems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SeatingChartApp.Runtime.UI
{
    /// <summary>
    /// A simple login manager that allows attendants to elevate to Admin
    /// privileges via a password.  Provides minimal feedback and the ability
    /// to log out back to Attendant.  In a production system this would be
    /// replaced with a secure authentication mechanism.
    /// </summary>
    public class LoginUIManager : MonoBehaviour
    {
        [Header("Login Panel References")]
        [SerializeField] public GameObject loginPanel;
        [SerializeField] public TMP_InputField passwordInput;
        [SerializeField] public Button loginButton;
        [SerializeField] public Button logoutButton;
        [SerializeField] public TMP_Text feedbackText;

        [Tooltip("Password required to enter Admin mode.  Change this in the inspector to configure your own password.")]
        public string adminPassword = "admin123";

        private void Awake()
        {
            if (loginButton != null) loginButton.onClick.AddListener(AttemptLogin);
            if (logoutButton != null) logoutButton.onClick.AddListener(Logout);
        }

        /// <summary>
        /// Shows the login panel if it is hidden.  Use this from a UI button
        /// elsewhere in your interface to allow attendants to request admin
        /// access.
        /// </summary>
        public void ShowLoginPanel()
        {
            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Attempts to authenticate the user as an admin.  If the provided
        /// password matches <see cref="adminPassword"/> then the role is
        /// switched to Admin and the login panel is hidden.  Otherwise a
        /// feedback message is shown.
        /// </summary>
        public void AttemptLogin()
        {
            if (passwordInput == null || UserRoleManager.Instance == null)
                return;
            if (passwordInput.text == adminPassword)
            {
                UserRoleManager.Instance.SetRole(UserRoleManager.Role.Admin);
                if (feedbackText != null) feedbackText.text = "Admin mode activated";
                if (loginPanel != null) loginPanel.SetActive(false);
                passwordInput.text = string.Empty;
            }
            else
            {
                if (feedbackText != null) feedbackText.text = "Incorrect password";
            }
        }

        /// <summary>
        /// Logs the user back down to Attendant mode and shows the login
        /// panel again.
        /// </summary>
        public void Logout()
        {
            if (UserRoleManager.Instance == null)
                return;
            UserRoleManager.Instance.SetRole(UserRoleManager.Role.Attendant);
            if (loginPanel != null) loginPanel.SetActive(true);
            if (feedbackText != null) feedbackText.text = string.Empty;
        }
    }
}