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
    public class SelectedRoomsArea : INumeratorRoom
    {
        public string Type { get; private set; }
        public List<Room> SelectRooms { get; private set; }
        public string SelectParameterName { get; set; }
        public string ResultMessege { get; private set; }
        public List<Room> ErrorRoom { get; private set; }
        public string StartValue { get; set; }
        private bool ascending;
        public SelectedRoomsArea(bool ascending = false)
        {
            this.ascending = ascending;
            if (ascending)
            {
                Type = "По возрастанию площади";
            }
            else
            {
                Type = "По убыванию площади";
            }
        }

        public List<Room> GetRooms(UIDocument UIDocument)
        {
            SelectRooms = Numerator.PickRooms(UIDocument);
            return SelectRooms;
        }

        public bool SetNumber()
        {
            string messege = String.Empty;
            List<Room> errorRoom = new List<Room>();
            bool isFilled = Numerator.SetNumberRoomArea(SelectRooms, SelectParameterName, ref messege, ref errorRoom, StartValue, ascending);
            ResultMessege = messege;
            ErrorRoom = errorRoom;
            return isFilled;
        }
    }
}
