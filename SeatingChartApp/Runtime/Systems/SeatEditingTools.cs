using SeatingChartApp.Runtime.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SeatingChartApp.Runtime.Systems
{
    /// <summary>
    /// Provides admin‑only tools for editing the seating layout at runtime.
    /// Supports adding new seats via a prefab and automatically assigns a
    /// unique identifier.  To enable seat dragging see <see cref="SeatController"/>.
    /// </summary>
    public class SeatEditingTools : MonoBehaviour
    {
        [Tooltip("Prefab used when creating new seats.")]
        public GameObject seatPrefab;
        [Tooltip("Parent RectTransform that new seats will be instantiated under.")]
        public RectTransform parentRect;
        [Tooltip("UI button that triggers the creation of a new seat.")]
        public Button addSeatButton;

        private void Awake()
        {
            if (addSeatButton != null)
            {
                addSeatButton.onClick.AddListener(AddNewSeat);
            }
        }

        /// <summary>
        /// Adds a new seat to the layout.  Only callable when the current
        /// role is Admin.  The seat is positioned at the centre of the
        /// parent rect and assigned a new ID.  The layout is marked dirty
        /// so it will be saved on the next update.
        /// </summary>
        public void AddNewSeat()
        {
            if (UserRoleManager.Instance == null || UserRoleManager.Instance.CurrentRole != UserRoleManager.Role.Admin)
                return;
            if (seatPrefab == null || parentRect == null)
                return;
            GameObject newSeatObj = Instantiate(seatPrefab, parentRect);
            SeatController seatController = newSeatObj.GetComponent<SeatController>();
            if (seatController == null)
            {
                Debug.LogError("Seat prefab is missing a SeatController component.");
                return;
            }
            seatController.SeatID = GenerateUniqueSeatID();
            // Position in the centre as a starting point
            RectTransform rect = newSeatObj.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            // Register with layout manager
            if (LayoutManager.Instance != null)
            {
                LayoutManager.Instance.Seats.Add(seatController);
                LayoutManager.Instance.MarkLayoutDirty();
            }
        }

        /// <summary>
        /// Generates a unique seat ID by incrementing from 1 until an unused
        /// value is found.  Assumes seat IDs are positive integers.
        /// </summary>
        /// <returns>A unique integer identifier.</returns>
        private int GenerateUniqueSeatID()
        {
            int id = 1;
            if (LayoutManager.Instance != null)
            {
                while (LayoutManager.Instance.Seats.Exists(s => s != null && s.SeatID == id))
                {
                    id++;
                }
            }
            return id;
        }
    }
}