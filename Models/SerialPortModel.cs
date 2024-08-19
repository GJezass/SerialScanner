using System;
using System.IO.Ports;

namespace SerialScanner.Models
{
    public class SerialPortModel
    {
        private SerialPort _serialPort;
        public event Action<string> DataReceived;

        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Connect(string portName, int baudRate = 9600)
        {
            if(_serialPort!=null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();
        }

        public void Disconnect()
        {
            if(_serialPort!= null && _serialPort.IsOpen)
            {
                _serialPort.DataReceived -= OnDataReceived;
                _serialPort.Close();
            }
        }

        public void SendData(string data)
        {
            if(_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Write(data);
            }
        }

        private string _buffer = string.Empty;

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadExisting();
            _buffer += data;

            string[] messages = _buffer.Split(new[] { '\n' }, StringSplitOptions.None);

            for (int i = 0; i< messages.Length - 1; i++)
            {
                DataReceived?.Invoke(messages[i]);
            }

            _buffer = messages[messages.Length - 1];
        }
    }
}
