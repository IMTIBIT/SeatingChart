using System;
using UnityEngine;

namespace SeatingChartApp.Runtime.Data
{
    /// <summary>
    /// Represents a single guest record.  This serializable class stores the
    /// personal details for an individual or party occupying a seat.  It
    /// intentionally avoids any behaviour so that it can be easily persisted
    /// with <see cref="JsonUtility"/>.
    /// </summary>
    [Serializable]
    public class GuestData
    {
        public string FirstName;
        public string LastName;
        public string RoomNumber;
        public int PartySize;

        public GuestData() { }

        public GuestData(string firstName, string lastName, string roomNumber, int partySize)
        {
            FirstName = firstName;
            LastName = lastName;
            RoomNumber = roomNumber;
            PartySize = partySize;
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName} (Room: {RoomNumber}, Party: {PartySize})";
        }
    }
}