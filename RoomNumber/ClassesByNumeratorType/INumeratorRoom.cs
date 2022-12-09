using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomNumber
{
    public interface INumeratorRoom
    {
        string Type { get; }

        List<Room> SelectRooms { get; }
        string SelectParameterName { get; set; }
        string StartValue { get; set; }
        List<Room> ErrorRoom { get; }
        string ResultMessege { get; }
        List<Room> GetRooms(UIDocument UIDocument);

        bool SetNumber();
    }
}
