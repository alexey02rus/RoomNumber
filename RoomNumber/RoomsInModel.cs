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
    public class RoomsInModel
    {

        public List<Room> Rooms { get; private set; } = new List<Room>(); // Все размещенные помещения в модели (без ошибок)
        public List<Room> ErrorRoom { get; private set; } = new List<Room>(); // Все помещения с ошибками (Неокруженные, неразмещенные, избыточные)

        public RoomsInModel(ExternalCommandData commandData)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            List<Room> allRoomInModel = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .OfType<Room>()
                .ToList();
            Rooms = allRoomInModel
                .Where(r => r.Area > 0)
                .ToList();
            ErrorRoom = allRoomInModel
                .Where(r => r.Area == 0)
                .ToList();
        }

        public List<Definition> GetParemetersRoomForSet(ref string messege) // Получение доступных для записи параметров помещений
        {
            if (!Rooms.Any())
            {
                messege = "В модели отсутствуют размещенные помещения!";
                return null;
            }

            List<Definition> defParameterSet = Rooms.First().Parameters
                .OfType<Parameter>()
                .Where(p => p.IsReadOnly == false)
                .Where(p => p.StorageType == StorageType.Double || p.StorageType == StorageType.Integer || p.StorageType == StorageType.String)
                .Select(p => p.Definition)
                .ToList();

            return defParameterSet;
        }

        //public List<Room> GetRoomsByParameteValue(string parameterName, string value, ref string messege)
        //{
        //    if (!Rooms.Any())
        //    {
        //        messege = "В модели отсутствуют размещенные помещения!";
        //        return null;
        //    }

        //    IList<Parameter> parameter = Rooms.First().GetParameters(parameterName);

        //    if (parameter.Any())
        //    {
        //        messege = "Указанный параметр отсутствует у помещений!";
        //        return null;
        //    }
        //    else if (parameter.Count() > 1)
        //    {
        //        messege = "Имеется более одного параметра с указанным именем!";
        //        return null;
        //    }

        //    List<Room> rooms = new List<Room>();
        //    StorageType parameterType = parameter.First().StorageType;
        //    if (parameterType == StorageType.String)
        //    {
        //        rooms = Rooms.Where(r => r.GetParameters(parameterName).First().AsString().Equals(value)).ToList();
        //    }
        //    else if (parameterType == StorageType.Integer)
        //    {
        //        if (!int.TryParse(value, out int valueInteget))
        //        {
        //            messege = $"Не удалось привести значение \"{value}\" к типу Integer!";
        //            return null;
        //        }
        //        rooms = Rooms.Where(r => r.GetParameters(parameterName).First().AsInteger().Equals(valueInteget)).ToList();
        //    }
        //    else if (parameterType == StorageType.Double)
        //    {
        //        if (!double.TryParse(value, out double valueDouble))
        //        {
        //            messege = $"Не удалось привести значение \"{value}\" к типу Double!";
        //            return null;
        //        }
        //        rooms = Rooms.Where(r => r.GetParameters(parameterName).First().AsDouble().Equals(valueDouble)).ToList();
        //    }
        //    else if (parameterType == StorageType.ElementId)
        //    {

        //    }
        //    else
        //    {

        //    }
        //    if (rooms.Count == 0)
        //    {
        //        messege = $"Не найдено помещений со значением \"{value}\" в параметре \"{parameterName}\"!";
        //    }
        //    return rooms;
        //}

    }
}
