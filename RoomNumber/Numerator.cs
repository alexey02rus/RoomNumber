using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoomNumber
{
    public static class Numerator
    {
        public static List<Room> PickRooms(UIDocument uiDocument) // Выбор помещений для нумерации пользователем
        {
            try
            {
                Document document = uiDocument.Document;
                IList<Reference> references = uiDocument.Selection.PickObjects(ObjectType.Element, new RoomFilter(), "Выберите помещения");
                List<Room> rooms = references.Select(r => document.GetElement(r) as Room).ToList();
                return rooms;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Исключение!", ex.Message);
                return null;
            }
        }

        public static List<Room> PickRooms(UIDocument uiDocument, out Room room) // Выбор помещений для нумерации пользователем и выбор стартового помещения
        {
            List<Room> rooms = Numerator.PickRooms(uiDocument);
            if (rooms == null || rooms.Count < 1)
            {
                room = null;
                return null;
            }
            try
            {
                Document document = uiDocument.Document;
                TaskDialog.Show("Выбор помещений", "Выберите помещениe, с которого необходимо начать нумерацию", TaskDialogCommonButtons.Ok);
                Reference reference = uiDocument.Selection.PickObject(ObjectType.Element, new RoomFilter(), "Выберите помещениe");
                room = document.GetElement(reference) as Room;
                return rooms;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                room = null;
                return null;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Исключение!", ex.Message);
                room = null;
                return null;
            }
        } 

        public static bool SetNumberRoomСlockwise(this List<Room> rooms, // Список помещений для нумерации
                                                  Room startRoom, // Помещение, с котрогоро необходимо начать нумерацию
                                                  string parameterName, // Имя параметра, в который необходимо внести номер помещения
                                                  ref string messege, // Сообщение о результате работы метода
                                                  ref List<Room> errorRooms, // Список элементов, для которых не удалось заполнить параметр
                                                  string startValue = "01", // Старторовое значение номера (при заполнении сохраняется форматирование количеством нулей)
                                                  bool counterclockWise = false) // Заполнять против часовой стрелки
        {
            if (rooms == null || rooms.Count < 1) // Проверка помещений
            {
                messege = "Ошибка! Список помещений для нумерации пуст!";
                return false;
            }

            int d = startValue.Length; // Количество указанных символов в номере для форматирования

            Document document = startRoom.Document;
            List<XYZ> points = rooms.Select(r => r.GetElementCenter()).ToList(); // Получение координат нумеруемых помещений
            XYZ startPoint = startRoom.GetElementCenter(); // Получение координаты помещения, с которого начинается нумерация
            XYZ centerPoint = points.Aggregate((f, s) => f + s) / rooms.Count; // Получение центральной координаты всех нумеруемых помещений, вокруг которой будет происходить нумерация
            XYZ startVector = startPoint - centerPoint; // Получение стартового вектора, от которого ведется отсчет
            Dictionary<double, Room> angleToRooms = new Dictionary<double, Room>(); // Объявление переменной, связывающей угол между стартовым помещением и нумеруемым
            foreach (Room room in rooms)
            {
                XYZ point = room.GetElementCenter();
                XYZ vector = point - centerPoint; // Получение вектора между центральной точкой и центральной координатой помещения
                double angle = startVector.AngleOnPlaneTo(vector, counterclockWise == false ? XYZ.BasisZ * -1 : XYZ.BasisZ); // Получение угла между стартовым вектором и вектором центральной точки помещения
                angleToRooms.Add(angle, room);
            }
            SortedDictionary<double, Room> SortedAngleToRooms = new SortedDictionary<double, Room>(angleToRooms); // Сортировка помещений по значению угла от начального помещения
            List<Room> sortedRoom = SortedAngleToRooms.Values.ToList();
            return SetValue(document, sortedRoom, parameterName, startValue, ref errorRooms, ref messege);
        }

        public static bool SetNumberRoomArea(this List<Room> rooms, // Список помещений для нумерации
                                                  string parameterName, // Имя параметра, в который необходимо внести номер помещения
                                                  ref string messege, // Сообщение о результате работы метода
                                                  ref List<Room> errorRooms, // Список элементов, для которых не удалось заполнить параметр
                                                  string startValue = "01", // Старторовое значение номера (при заполнении сохраняется форматирование количеством нулей)
                                                  bool ascending = false) // Заполнять по возрастанию площади
        {
            if (rooms == null || rooms.Count < 1) // Проверка помещений
            {
                messege = "Ошибка! Список помещений для нумерации пуст!";
                return false;
            }

            int d = startValue.Length; // Количество указанных символов в номере для форматирования
            List<Room> sortedRoom;
            Document document = rooms.First().Document;
            if (ascending)
            {
                sortedRoom = rooms.OrderBy(r => r.Area).ToList();
            }
            else
            {
                sortedRoom = rooms.OrderByDescending(r => r.Area).ToList();
            }

            return SetValue(document, sortedRoom, parameterName, startValue, ref errorRooms, ref messege);
        }


        private static XYZ GetElementCenter(this Element element) // Метод, получающий координату центра помещения
        {
            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            XYZ centralPoint = (boundingBox.Max + boundingBox.Min) / 2;
            return centralPoint;
        }
        private static bool ParameterIsCorrectForSet(this Element element,
                                                     string parameterName,
                                                     ref string messege,
                                                     out Parameter parameter) // Метод, проверяющий параметр элемента на возможность записи значения
        {
            IList<Parameter> parameters = element.GetParameters(parameterName);
            parameter = null;

            if (!parameters.Any())
            {
                messege = "Указанный параметр отсутствует у помещений!";
                return false;
            }
            else if (parameters.Count() > 1)
            {
                if (parameters.Select(p => p.StorageType).Distinct().Count() == 1)
                {
                    messege = "Имеется более одного параметра с указанным именем и одинаковым типом данных!";
                    return false;
                }
                else
                {
                    foreach (Parameter param in parameters)
                    {
                        if (param.IsReadOnly)
                        {
                            if (parameters.Last().Equals(param))
                            {
                                messege = "Указанный параметр доступен только для чтения!";
                            }
                            continue;
                        }
                        if (param.StorageType == StorageType.Integer)
                        {
                            parameter = param;
                            messege = "Имеется более одного параметра с указанным именем! Значение будет записано в параметр с типом данных \"Целое\"";
                            break;
                        }
                        else if (param.StorageType == StorageType.Double)
                        {
                            parameter = param;
                            messege = "Имеется более одного параметра с указанным именем! Значение будет записано в параметр с типом данных \"Число\"";
                            break;
                        }
                        else if (param.StorageType == StorageType.String)
                        {
                            parameter = param;
                            messege = "Имеется более одного параметра с указанным именем! Значение будет записано в параметр с типом данных \"Текст\"";
                            break;
                        }
                        else
                        {
                            if (parameters.Last().Equals(param))
                            {
                                messege = "Указанный параметр имеет недопустимый тип данных!";
                            }
                            continue;
                        }
                    }
                }
            }
            else
            {
                if (parameters.First().IsReadOnly)
                {
                    messege = "Указанный параметр доступен только для чтения!";
                }
                parameter = parameters.First();
                return true;
            }
            if (parameter == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static bool SetValue(Document document,
                                         List<Room> sortedRoom,
                                         string parameterName,
                                         string startValue,
                                         ref List<Room> errorRooms,
                                         ref string messege)
        {
            ElementId idParameter; // id корректного параметра
            if (sortedRoom.First().ParameterIsCorrectForSet(parameterName, ref messege, out Parameter parameter)) // Проверка корректности параметра
            {
                idParameter = parameter.Id;
            }
            else
            {
                return false;
            }

            bool isNumber = int.TryParse(startValue, out int startNumber); // Проверка начального номера
            if (!isNumber)
            {
                messege = "Не удалось преобразовать стартовое значение номера в целое число!";
                return false;
            }
            try
            {
                using (Transaction ts = new Transaction(document, "Нумерация помещений")) // Заполнение значений 
                {
                    ts.Start();
                    foreach (Room room in sortedRoom)
                    {
                        Parameter param = room.GetParameters(parameterName).Where(p => p.Id.IntegerValue == idParameter.IntegerValue).First();
                        if (param.IsReadOnly) // Если у элемента параметр окажется только для чтения, элемент будет пропущен и занесен в список
                        {
                            errorRooms.Add(room);
                            continue;
                        }
                        else
                        {
                            if (param.StorageType == StorageType.Double || param.StorageType == StorageType.Integer) // Если тип данных число, просто записать
                            {
                                param.Set(startNumber);
                            }
                            else if (param.StorageType == StorageType.String) // Если тип данных текст, то форматировать до нужного количества символов и записать
                            {
                                string valueFormat = startNumber.ToString($"d{startValue.Length}");
                                param.Set(valueFormat);
                            }
                        }
                        startNumber++;
                    }
                    ts.Commit();
                    if (errorRooms.Any())
                    {
                        messege = $"Заполнение выполнено частично! Не удалось заполнить значение для следющих {errorRooms.Count} помещений: ";
                        messege += string.Join(", ", errorRooms.Select(r => r.Id.ToString() as string));
                    }
                    else
                    {
                        messege = "Заполнение выполнено успешно!";
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                messege = ex.Message;
                return false;
            }

        }
    }
}
