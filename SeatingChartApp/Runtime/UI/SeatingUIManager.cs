using System;
using SeatingChartApp.Runtime.Data;
using SeatingChartApp.Runtime.Systems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SeatingChartApp.Runtime.UI
{
    /// <summary>
    /// Central controller for the guest assignment user interface.  This
    /// manager opens and closes the assignment panel, reads user input, and
    /// applies assignments back onto the selected <see cref="SeatController"/>.
    /// It also exposes an "Assign Previous" function which reuses the last
    /// entered guest without typing.
    /// </summary>
    public class SeatingUIManager : MonoBehaviour
    {
        public static SeatingUIManager Instance { get; private set; }

        [Header("Assignment Panel References")]
        public GameObject assignmentPanel;
        public TMP_InputField firstNameInput;
        public TMP_InputField lastNameInput;
        public TMP_InputField roomNumberInput;
        public TMP_InputField partySizeInput;
        public Button assignButton;
        public Button clearButton;
        public Button assignPreviousButton;
        public Button toggleOutOfServiceButton;


        public SeatController _activeSeat;
        // Optional text field to display errors such as capacity overflows
        public TMP_Text errorText;
        public GuestData _lastAssignedGuest;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (assignmentPanel != null)
            {
                assignmentPanel.SetActive(false);
            }
            // Hook button callbacks if assigned
            if (assignButton != null) assignButton.onClick.AddListener(OnAssignButton);
            if (clearButton != null) clearButton.onClick.AddListener(OnClearButton);
            if (assignPreviousButton != null) assignPreviousButton.onClick.AddListener(OnAssignPreviousButton);
            if (toggleOutOfServiceButton != null) toggleOutOfServiceButton.onClick.AddListener(OnToggleOutOfServiceButton);
        }

        /// <summary>
        /// Opens the assignment panel for the provided seat.  The panel
        /// populates controls and toggles their interactability based on the
        /// seat's current state.  The active seat is stored so actions can
        /// apply back to it when the user presses buttons.
        /// </summary>
        public void OpenSeatAssignmentPanel(SeatController seat)
        {
            if (seat == null)
                return;
            _activeSeat = seat;
            if (assignmentPanel != null)
            {
                assignmentPanel.SetActive(true);
            }
            // Reset input fields
            if (firstNameInput != null) firstNameInput.text = string.Empty;
            if (lastNameInput != null) lastNameInput.text = string.Empty;
            if (roomNumberInput != null) roomNumberInput.text = string.Empty;
            if (partySizeInput != null) partySizeInput.text = string.Empty;
            // Set button states
            if (clearButton != null) clearButton.interactable = seat.State != SeatState.Available && seat.State != SeatState.OutOfService;
            if (assignPreviousButton != null) assignPreviousButton.interactable = _lastAssignedGuest != null;
            if (toggleOutOfServiceButton != null)
            {
                var textComp = toggleOutOfServiceButton.GetComponentInChildren<TMP_Text>();
                if (seat.State == SeatState.OutOfService)
                {
                    if (textComp != null) textComp.text = "Set Available";
                }
                else
                {
                    if (textComp != null) textComp.text = "Out of Service";
                }
            }
            if (errorText != null) errorText.text = string.Empty;
        }

        /// <summary>
        /// Closes the assignment panel and clears the active seat reference.
        /// </summary>
        private void ClosePanel()
        {
            if (assignmentPanel != null)
                assignmentPanel.SetActive(false);
            _activeSeat = null;
        }

        /// <summary>
        /// Callback invoked when the Assign button is pressed.  Creates a new
        /// <see cref="GuestData"/> from the UI fields, assigns it to the
        /// active seat and stores it as the last assigned guest for reuse.
        /// </summary>
        public void OnAssignButton()
        {
            if (_activeSeat == null)
                return;
            // Create guest record from inputs.  If inputs are invalid the
            // resulting data will be empty or zero – further validation
            // could be added here.
            string firstName = firstNameInput != null ? firstNameInput.text : string.Empty;
            string lastName = lastNameInput != null ? lastNameInput.text : string.Empty;
            string room = roomNumberInput != null ? roomNumberInput.text : string.Empty;
            int partySize = 0;
            if (partySizeInput != null)
                int.TryParse(partySizeInput.text, out partySize);
            var guest = new GuestData(firstName, lastName, room, partySize);
            // Capacity check: ensure the seat can accommodate the party
            if (!_activeSeat.CanAssignGuest(guest))
            {
                if (errorText != null)
                    errorText.text = $"This seat cannot accommodate a party of {guest.PartySize}. Capacity: {_activeSeat.Capacity}";
                return;
            }
            if (errorText != null) errorText.text = string.Empty;
            _activeSeat.AssignGuest(guest);
            _lastAssignedGuest = guest;
            // Close panel after assignment
            ClosePanel();
        }

        /// <summary>
        /// Callback invoked when the Clear button is pressed.  Clears the
        /// active seat and hides the panel.
        /// </summary>
        public void OnClearButton()
        {
            if (_activeSeat == null)
                return;
            _activeSeat.ClearSeat();
            ClosePanel();
        }

        /// <summary>
        /// Callback invoked when the Assign Previous button is pressed.  If a
        /// last assigned guest exists it is cloned onto the active seat.
        /// </summary>
        public void OnAssignPreviousButton()
        {
            if (_activeSeat == null || _lastAssignedGuest == null)
                return;
            // Clone the guest so each seat has its own record
            var cloned = new GuestData(_lastAssignedGuest.FirstName, _lastAssignedGuest.LastName, _lastAssignedGuest.RoomNumber, _lastAssignedGuest.PartySize);
            if (!_activeSeat.CanAssignGuest(cloned))
            {
                if (errorText != null)
                    errorText.text = $"This seat cannot accommodate a party of {cloned.PartySize}. Capacity: {_activeSeat.Capacity}";
                return;
            }
            if (errorText != null) errorText.text = string.Empty;
            _activeSeat.AssignGuest(cloned);
            ClosePanel();
        }

        /// <summary>
        /// Callback for the Out‑of‑Service toggle button.  Toggles the seat's
        /// out‑of‑service flag and hides the panel.
        /// </summary>
        public void OnToggleOutOfServiceButton()
        {
            if (_activeSeat == null)
                return;
            _activeSeat.ToggleOutOfService();
            ClosePanel();
        }
    }
}