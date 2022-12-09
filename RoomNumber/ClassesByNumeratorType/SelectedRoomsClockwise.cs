using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomNumber
{
    public class SelectedRoomsClockwise : INumeratorRoom
    {
        public string Type { get; private set; }
        public List<Room> SelectRooms { get; private set; }
        public string SelectParameterName { get; set; }
        public Room StartRoom { get; private set; }
        public string ResultMessege { get; private set; }
        public List<Room> ErrorRoom { get; private set; }
        public string StartValue { get; set; }
        private bool counterclockWise;
        public SelectedRoomsClockwise(bool counterclockWise = false)
        {
            this.counterclockWise = counterclockWise;
            if (counterclockWise)
            {
                Type = "Против часовой стрелки";
            }
            else
            {
                Type = "По часовой стрелке";
            }
        }

        public List<Room> GetRooms(UIDocument UIDocument)
        {
            SelectRooms = Numerator.PickRooms(UIDocument, out Room startRoom);
            StartRoom = startRoom;
            return SelectRooms;
        }

        public bool SetNumber()
        {
            string messege = String.Empty;
            List<Room> errorRoom = new List<Room>();
            bool isFilled = Numerator.SetNumberRoomСlockwise(SelectRooms, StartRoom, SelectParameterName, ref messege, ref errorRoom, StartValue, counterclockWise);
            ResultMessege = messege;
            ErrorRoom = errorRoom;
            return isFilled;
        }
    }
}
