using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RoomNumber
{
    public class MainViewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }

        private ExternalCommandData commandData;

        public DelegateCommand MainCommand { get; private set; }
        public DelegateCommand SelectCommand { get; private set; }

        public List<INumeratorRoom> NumeratorType { get; private set; } = new List<INumeratorRoom>();
        private INumeratorRoom selectedNumeratorType;
        public INumeratorRoom SelectedNumeratorType 
        {
            get => selectedNumeratorType;
            set
            {
                if (!value.Equals(selectedNumeratorType))
                {
                    SelectElement.Clear();
                    SelectElementInfo = "Помещения не выбраны.";
                }
                selectedNumeratorType = value;
                OnPropertyChanged();
            } 
        }

        public List<Definition> Parameters { get; private set; } = new List<Definition>();
        private Definition selectParameter;
        public Definition SelectedParameter
        {
            get => selectParameter;
            set
            {
                selectParameter = value;
                OnPropertyChanged();
            }
        }

        private string sartNumber;
        public string StartNumber
        {
            get => sartNumber;
            set
            {
                sartNumber = value;
                OnPropertyChanged();
            }
        }

        private string selectElementInfo = "Помещения не выбраны.";
        public string SelectElementInfo
        {
            get => selectElementInfo;
            set
            {
                selectElementInfo = value;
                OnPropertyChanged();
            }
        }

        private List<Room> selectElement = new List<Room>();
        public List<Room> SelectElement 
        {
            get => selectElement;
            set
            {
                selectElement = value;
                OnPropertyChanged();
            }
        }

        private string messege;
        public string Messege
        {
            get => messege;
            set
            {
                messege = value;
                OnPropertyChanged();
            }
        }

        private bool successfully = false;
        public bool Successfully
        {
            get => successfully;
            set
            {
                successfully = value;
                OnPropertyChanged();
            }
        }

        public MainViewViewModel(ExternalCommandData commandData)
        {

            this.commandData = commandData;
            MainCommand = new DelegateCommand(OnMainCommand, CanExecuteMainCommand)
                .ObservesProperty<string>(() => StartNumber)
                .ObservesProperty<Definition>(() => SelectedParameter)
                .ObservesProperty<INumeratorRoom>(() => SelectedNumeratorType)
                .ObservesProperty<List<Room>>(() => SelectElement);

            SelectCommand = new DelegateCommand(OnSelectCommand, CanExecuteSelectCommand)
                .ObservesProperty<INumeratorRoom>(() => SelectedNumeratorType);

            NumeratorType = new List<INumeratorRoom>
            {
                new SelectedRoomsClockwise(),
                new SelectedRoomsClockwise(true),
                new SelectedRoomsArea(),
                new SelectedRoomsArea(true)
            };
            SelectedNumeratorType = NumeratorType.First();
            RoomsInModel roomsInModel = new RoomsInModel(commandData);
            string errorMessege = string.Empty;
            Parameters = roomsInModel.GetParemetersRoomForSet(ref errorMessege);
            Messege = errorMessege;
            SelectedParameter = Parameters.Where(p => ((int)(p as InternalDefinition).BuiltInParameter) == (int)BuiltInParameter.ROOM_NUMBER).FirstOrDefault();
            StartNumber = "01";
        }

        private bool CanExecuteMainCommand()
        {
            bool startNumberIsCorrect = int.TryParse(StartNumber, out int startNumber);
            if (startNumberIsCorrect)
            {
                Messege = string.Empty;
            }
            else
            {
                Messege = "Не удалось преобразовать стартовое значение в число!";
            }
            return SelectedNumeratorType != null
                   && SelectedParameter != null
                   && SelectElement != null
                   && SelectElement.Count > 0
                   && startNumberIsCorrect;
        }

        private bool CanExecuteSelectCommand()
        {
            return SelectedNumeratorType != null;
        }

        private void OnSelectCommand()
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            RaiseHideRequest();
            SelectElement.Clear();
            SelectElement = SelectedNumeratorType.GetRooms(uiDoc);
            if (SelectElement.Any())
            {
                SelectElementInfo = $"Выбрано {SelectElement.Count} помещений.";
            }
            else
            {
                SelectElementInfo = "Помещения не выбраны.";
            }
            RaiseShowRequest();
        }


        private void OnMainCommand()
        {
            SelectedNumeratorType.SelectParameterName = SelectedParameter.Name;
            SelectedNumeratorType.StartValue = StartNumber;
            Successfully = SelectedNumeratorType.SetNumber();
            Messege = SelectedNumeratorType.ResultMessege;
        }
    }
}
