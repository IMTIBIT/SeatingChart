using System.Collections.Generic;
using UnityEngine;

namespace SeatingChartApp.Runtime.Systems
{
    /// <summary>
    /// Utility component that validates the presence of all required systems
    /// when the scene loads.  If any critical component is missing it
    /// logs a warning to the console to help developers catch setup errors.
    /// Attach this to a singleton GameObject in your scene.
    /// </summary>
    public class SceneSetupValidator : MonoBehaviour
    {
        private void Start()
        {
            CheckForComponent<LayoutManager>();
            CheckForComponent<UserRoleManager>();
            CheckForComponent(typeof(SeatingChartApp.Runtime.UI.SeatingUIManager));
            CheckForComponent(typeof(SeatingChartApp.Runtime.UI.LoginUIManager));
            CheckForSeats();
        }

        private void CheckForComponent<T>() where T : Object
        {
            if (FindObjectOfType<T>() == null)
            {
                Debug.LogWarning($"SceneSetupValidator: Required component '{typeof(T).Name}' is missing from the scene.");
            }
        }

        private void CheckForComponent(System.Type t)
        {
            if (FindObjectOfType(t) == null)
            {
                Debug.LogWarning($"SceneSetupValidator: Required component '{t.Name}' is missing from the scene.");
            }
        }

        private void CheckForSeats()
        {
            var seats = FindObjectsOfType<SeatController>();
            if (seats == null || seats.Length == 0)
            {
                Debug.LogWarning("SceneSetupValidator: No SeatController instances were found.  The seating chart cannot function without seats.");
            }
        }
    }
}