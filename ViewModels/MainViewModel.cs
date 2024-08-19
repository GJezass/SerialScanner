using SerialScanner.Commands;
using SerialScanner.Models;
using SerialScanner.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Input;

namespace SerialScanner.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        private SerialPortModel _serialPortModel;
        private string _selectedPort;
        private string _portName;
        private int _selectedBaudRate;
        private string _dataToSend;
        private string _receivedData;
        private bool _isConnected;
        private int _messageCount;

        public ObservableCollection<string> AvailablePorts { get; set; }
        public ObservableCollection<int> AvailableBaudRates { get; set; }

        
        public ICommand ScanCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public ICommand ClearCommand { get; set; }


        public MainViewModel() {
            _serialPortModel= new SerialPortModel();
            _serialPortModel.DataReceived += OnDataReceived;
            
            AvailablePorts = new ObservableCollection<string>(_serialPortModel.GetAvailablePorts());
            AvailableBaudRates = new ObservableCollection<int> { 9600, 19200, 38400, 57600, 115200 };
            SelectedBaudRate = 0;
            
            ScanCommand = new RelayCommand(o => ScanPorts());
            ConnectCommand = new RelayCommand(o => ConnectToPort(), o => CanConnectToPort());
            SendCommand = new RelayCommand(o => SendData(), o => CanSendData());
            ClearCommand = new RelayCommand(o => ClearMessages());

        }
        
        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                _selectedPort = value;
                OnPropertyChanged(nameof(SelectedPort));
                OnPropertyChanged(nameof(CanConnectToPort));
            }
        }

        public string PortName
        {
            get => _portName;
            set
            {
                _portName = value;
                OnPropertyChanged(nameof(PortName));
            }
        }

        public int SelectedBaudRate
        {
            get => _selectedBaudRate;
            set
            {
                _selectedBaudRate = value;
                OnPropertyChanged(nameof(SelectedBaudRate));
            }
        }

        public string DataToSend
        {
            get => _dataToSend;
            set
            {
                _dataToSend = value;
                OnPropertyChanged(nameof(DataToSend));
            }
        }

        public string ReceivedData
        {
            get => _receivedData;
            set
            {
                _receivedData = value;
                OnPropertyChanged(nameof(ReceivedData));
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value; 
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public int MessageCount
        {
            get => _messageCount;
            set
            {
                _messageCount = value;
                OnPropertyChanged(nameof(MessageCount));
            }
        }

        private void ScanPorts()
        {
            AvailablePorts.Clear();
            foreach(var port in _serialPortModel.GetAvailablePorts())
            {
                AvailablePorts.Add(port);
            }
        }

        private void ConnectToPort()
        {
            if(IsConnected)
            {
                _serialPortModel.Disconnect();
                IsConnected = false;
                ReceivedData += "Disconnected from port.\n";
                PortName= string.Empty;
            }
            else
            {
                try
                {
                    _serialPortModel.Connect(SelectedPort, SelectedBaudRate);
                    IsConnected = true;
                    ReceivedData += $"Connected to port {SelectedPort}.\n";
                    PortName = SelectedPort;
                }
                catch (Exception ex)
                {
                    ReceivedData += $"Error: {ex.Message}\n";
                }
            }
        }

        private bool CanConnectToPort()
        {
            return !string.IsNullOrEmpty(SelectedPort) && SelectedBaudRate > 0;
        }

        private void SendData()
        {
            _serialPortModel.SendData(SelectedPort);
            ReceivedData += $"{DateTime.Now} | Tx: {DataToSend}\n";
            MessageCount++;
        }

        private bool CanSendData()
        {
            return IsConnected && !string.IsNullOrEmpty(DataToSend);
        }

        private void ClearMessages()
        {
            ReceivedData = string.Empty;
            MessageCount= 0;
        }

        private void OnDataReceived(string data)
        {
            ReceivedData += $"{DateTime.Now} | Rx: {data}\n";
            OnPropertyChanged(nameof(ReceivedData));
            MessageCount++;
        }
    }
}
