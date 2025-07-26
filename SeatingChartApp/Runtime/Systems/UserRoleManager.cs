using System;
using UnityEngine;

namespace SeatingChartApp.Runtime.Systems
{
    /// <summary>
    /// Manages the current user's role (Attendant or Admin) and broadcasts
    /// changes to interested listeners.  Roles control access to editing
    /// features and UI visibility throughout the app.
    /// </summary>
    public class UserRoleManager : MonoBehaviour
    {
        public static UserRoleManager Instance { get; private set; }

        /// <summary>
        /// Defines the available user roles.  Attendants are limited to
        /// assigning guests and clearing seats, while Admins gain access to
        /// layout editing and out‑of‑service toggles.
        /// </summary>
        public enum Role
        {
            Attendant,
            Admin
        }

        /// <summary>
        /// Event invoked whenever the current role changes.
        /// </summary>
        public event Action<Role> OnRoleChanged;

        /// <summary>
        /// The current active role.  Defaults to Attendant on startup.
        /// </summary>
        public Role CurrentRole { get; private set; } = Role.Attendant;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Sets the active role.  If the role actually changes, the
        /// <see cref="OnRoleChanged"/> event is invoked.
        /// </summary>
        public void SetRole(Role newRole)
        {
            if (CurrentRole == newRole)
                return;
            CurrentRole = newRole;
            OnRoleChanged?.Invoke(newRole);
        }
    }
}