using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SeatingChartApp.Runtime.Data;
using SeatingChartApp.Runtime.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeatingChartApp.Runtime.UI
{
    /// <summary>
    /// Provides a collection of administrator‑only tools accessible via UI
    /// controls.  These include clearing or bulk toggling all seats, searching
    /// and filtering by guest or state, exporting a utilisation report and
    /// threshold‑based alerting.  Attach this to a convenient UI canvas and
    /// hook up the referenced components via the inspector.
    /// </summary>
    public class AdminToolsManager : MonoBehaviour
    {
        [Header("General References")]
        [SerializeField] public LayoutManager layoutManager;
        [SerializeField] public GameObject confirmClearPanel;
        [SerializeField] public Button confirmClearYesButton;
        [SerializeField] public Button confirmClearNoButton;
        public GameObject adminToolsPanel;


        [Header("Clear / Bulk Toggle")]
        [SerializeField] public Button clearAllButton;
        [SerializeField] public Button bulkToggleButton;
        [SerializeField] public TMP_Dropdown bulkStateDropdown;

        [Header("Search & Filter")]
        [SerializeField] public TMP_InputField searchInput;
        [SerializeField] public Button searchButton;
        [SerializeField] public TMP_Dropdown filterDropdown;

        [Header("Reporting")]
        [SerializeField] public Button exportReportButton;
        [Tooltip("Filename for the CSV session report.  Will be saved into Application.persistentDataPath.")]
        public string reportFilename = "session_report.csv";

        [Header("Alerts")]
        [Tooltip("Occupancy duration in minutes after which a seat will trigger an alert.")]
        public float alertThresholdMinutes = 90f;

        private void Awake()
        {
            // Hook up buttons
            if (clearAllButton != null) clearAllButton.onClick.AddListener(OnClearAllPressed);
            if (bulkToggleButton != null) bulkToggleButton.onClick.AddListener(OnBulkTogglePressed);
            if (searchButton != null) searchButton.onClick.AddListener(OnSearchPressed);
            if (filterDropdown != null) filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            if (exportReportButton != null) exportReportButton.onClick.AddListener(OnExportReportPressed);
            if (confirmClearYesButton != null) confirmClearYesButton.onClick.AddListener(ConfirmClearAll);
            if (confirmClearNoButton != null) confirmClearNoButton.onClick.AddListener(CancelClearAll);
            if (confirmClearPanel != null) confirmClearPanel.SetActive(false);
            // Hide bulk toggle UI when no state dropdown assigned
            if (bulkStateDropdown != null)
            {
                // Populate the dropdown with seat state names
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (string name in Enum.GetNames(typeof(SeatState)))
                {
                    options.Add(new TMP_Dropdown.OptionData(name));
                }
                bulkStateDropdown.options = options;
            }
        }

        #region Clear / Bulk
        /// <summary>
        /// Invoked when the Clear All button is pressed.  Only displays the
        /// confirmation panel; the actual clearing happens on confirm.
        /// </summary>
        private void OnClearAllPressed()
        {
            if (UserRoleManager.Instance == null || UserRoleManager.Instance.CurrentRole != UserRoleManager.Role.Admin)
                return;
            if (confirmClearPanel != null)
                confirmClearPanel.SetActive(true);
        }

        /// <summary>
        /// Confirms the clear all operation.  Iterates through every seat,
        /// clears them and triggers a save.  Closes the confirmation panel
        /// afterwards.
        /// </summary>
        private void ConfirmClearAll()
        {
            if (layoutManager == null) layoutManager = LayoutManager.Instance;
            if (layoutManager == null)
                return;
            foreach (SeatController seat in layoutManager.Seats)
            {
                if (seat != null)
                {
                    seat.ClearSeat();
                }
            }
            layoutManager.SaveLayout();
            if (confirmClearPanel != null) confirmClearPanel.SetActive(false);
        }

        private void CancelClearAll()
        {
            if (confirmClearPanel != null)
                confirmClearPanel.SetActive(false);
        }

        /// <summary>
        /// Invoked when the Bulk Toggle button is pressed.  Applies the
        /// selected state from the dropdown to all seats.  Marks the layout
        /// dirty so it will be saved on the next update.
        /// </summary>
        private void OnBulkTogglePressed()
        {
            if (UserRoleManager.Instance == null || UserRoleManager.Instance.CurrentRole != UserRoleManager.Role.Admin)
                return;
            if (bulkStateDropdown == null)
                return;
            int index = bulkStateDropdown.value;
            SeatState targetState = (SeatState)index;
            if (layoutManager == null) layoutManager = LayoutManager.Instance;
            if (layoutManager == null)
                return;
            foreach (SeatController seat in layoutManager.Seats)
            {
                if (seat == null)
                    continue;
                seat.CurrentGuest = null;
                seat.State = targetState;
                if (seat.TimerText != null) seat.TimerText.text = string.Empty;
                seat.UpdateVisualState();
            }
            layoutManager.MarkLayoutDirty();
        }
        #endregion

        #region Search & Filter
        /// <summary>
        /// Invoked when the Search button is pressed.  Looks through all
        /// occupied seats for a guest whose first name, last name or room
        /// number contains the search text (case‑insensitive).  If found,
        /// highlights the seat by scaling it briefly.  If not found, no
        /// feedback is given.
        /// </summary>
        private void OnSearchPressed()
        {
            if (layoutManager == null) layoutManager = LayoutManager.Instance;
            if (layoutManager == null || searchInput == null)
                return;
            string query = searchInput.text;
            if (string.IsNullOrEmpty(query))
                return;
            query = query.ToLowerInvariant();
            foreach (SeatController seat in layoutManager.Seats)
            {
                if (seat == null || seat.CurrentGuest == null)
                    continue;
                string fullName = ($"{seat.CurrentGuest.FirstName} {seat.CurrentGuest.LastName}").ToLowerInvariant();
                string room = seat.CurrentGuest.RoomNumber != null ? seat.CurrentGuest.RoomNumber.ToLowerInvariant() : string.Empty;
                if (fullName.Contains(query) || room.Contains(query))
                {
                    HighlightSeat(seat);
                    return;
                }
            }
        }

        /// <summary>
        /// Applies a simple highlight effect by briefly increasing the scale of
        /// the seat's transform.  A coroutine returns it to normal after a
        /// short delay.
        /// </summary>
        private void HighlightSeat(SeatController seat)
        {
            var t = seat.transform as RectTransform;
            if (t == null)
                return;
            StopAllCoroutines();
            StartCoroutine(HightlightRoutine(t));
        }

        private System.Collections.IEnumerator HightlightRoutine(RectTransform rect)
        {
            Vector3 originalScale = rect.localScale;
            Vector3 targetScale = originalScale * 1.3f;
            float duration = 0.2f;
            float time = 0f;
            // Scale up
            while (time < duration)
            {
                rect.localScale = Vector3.Lerp(originalScale, targetScale, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            rect.localScale = targetScale;
            // Hold for a moment
            yield return new WaitForSecondsRealtime(0.3f);
            // Scale back
            time = 0f;
            while (time < duration)
            {
                rect.localScale = Vector3.Lerp(targetScale, originalScale, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            rect.localScale = originalScale;
        }

        /// <summary>
        /// Invoked when the filter dropdown value changes.  Shows or hides
        /// seats based on the selected state.  An "All" option (index 0)
        /// shows every seat.
        /// </summary>
        private void OnFilterChanged(int value)
        {
            if (layoutManager == null) layoutManager = LayoutManager.Instance;
            if (layoutManager == null)
                return;
            SeatState? filter = null;
            // We expect the filter dropdown to contain an "All" option at
            // index 0 followed by the SeatState names.  If more complex
            // filtering is required you can extend this logic accordingly.
            if (value > 0)
            {
                filter = (SeatState)(value - 1);
            }
            foreach (SeatController seat in layoutManager.Seats)
            {
                if (seat == null)
                    continue;
                bool show = true;
                if (filter.HasValue)
                {
                    show = seat.State == filter.Value;
                }
                seat.gameObject.SetActive(show);
            }
        }
        #endregion

        #region Reporting
        /// <summary>
        /// Exports a simple CSV report summarising each seat's utilisation
        /// during the current session.  Writes to persistent data path with
        /// <see cref="reportFilename"/>.  The report includes the seat ID,
        /// state, current guest (if any), occupancy start time and elapsed
        /// minutes.
        /// </summary>
        private void OnExportReportPressed()
        {
            if (layoutManager == null) layoutManager = LayoutManager.Instance;
            if (layoutManager == null)
                return;
            var sb = new StringBuilder();
            sb.AppendLine("Seat ID,State,Guest Name,Room,Party Size,Occupied Minutes");
            foreach (SeatController seat in layoutManager.Seats)
            {
                if (seat == null)
                    continue;
                string guestName = seat.CurrentGuest != null ? $"{seat.CurrentGuest.FirstName} {seat.CurrentGuest.LastName}" : string.Empty;
                string room = seat.CurrentGuest != null ? seat.CurrentGuest.RoomNumber : string.Empty;
                string party = seat.CurrentGuest != null ? seat.CurrentGuest.PartySize.ToString() : string.Empty;
                float minutes = 0f;
                if (seat.State == SeatState.Occupied)
                {
                    minutes = (Time.time - seat.OccupiedStartTime) / 60f;
                }
                sb.AppendLine($"{seat.SeatID},{seat.State},{guestName},{room},{party},{minutes:F1}");
            }
            string path = Path.Combine(Application.persistentDataPath, reportFilename);
            try
            {
                File.WriteAllText(path, sb.ToString());
                Debug.Log($"Session report exported to: {path}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to export report: {ex.Message}");
            }
        }
        #endregion

        private void Update()
        {
            // Iterate seats once per frame and mark those exceeding the alert
            // threshold for visual feedback.  This approach centralises
            // alert logic in a single place rather than burying it in
            // SeatController.Update.
            if (layoutManager == null) layoutManager = LayoutManager.Instance;
            if (layoutManager == null)
                return;
            float thresholdSeconds = alertThresholdMinutes * 60f;
            foreach (SeatController seat in layoutManager.Seats)
            {
                if (seat == null)
                    continue;
                if (seat.State == SeatState.Occupied)
                {
                    float elapsed = Time.time - seat.OccupiedStartTime;
                    if (elapsed >= thresholdSeconds)
                    {
                        // Flash the seat by toggling its alpha using PingPong
                        if (seat.SeatImage != null)
                        {
                            Color c = seat.SeatImage.color;
                            float ping = Mathf.PingPong(Time.unscaledTime * 2f, 1f);
                            c.a = Mathf.Lerp(0.5f, 1f, ping);
                            seat.SeatImage.color = c;
                        }
                    }
                    else
                    {
                        // Ensure full alpha when below threshold
                        if (seat.SeatImage != null)
                        {
                            Color c = seat.SeatImage.color;
                            c.a = 1f;
                            seat.SeatImage.color = c;
                        }
                    }
                }
            }
        }
    }
}