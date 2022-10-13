using System;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;
using Shared.Log;

namespace ClientSide.Bike;

public class BluetoothDevice
{
    public bool Connected = false;
    
    private BLE ble;
    private string deviceName;
    private string? service;
    private string? characteristic;
    private BLESubscriptionValueChangedEventHandler? valueChanged;


    public BluetoothDevice(string name, string? service, string? characteristic, BLESubscriptionValueChangedEventHandler? valueChanged)
    {
        deviceName = name;
        this.service = service;
        this.characteristic = characteristic;
        this.valueChanged = valueChanged;
        ble = new BLE();

    }

    /// <summary>
    /// It opens a connection to the device, sets the service, subscribes to the characteristic, and then prints if the connection
    /// has been made.
    /// </summary>
    public async Task StartConnection()
    {
        Logger.LogMessage(LogImportance.Debug, $"Starting connection of device {deviceName}");
        //Thread.Sleep(500);
        int errorCode = 0;
        errorCode = await ble.OpenDevice(deviceName);
        CheckErrorCode(errorCode, "OpenDevice");
        if (service != null)
        {
            errorCode = await ble.SetService(service);
            CheckErrorCode(errorCode, "Service");
            ble.SubscriptionValueChanged += valueChanged;
            if (characteristic != null)
            {
                errorCode = await ble.SubscribeToCharacteristic(characteristic);
                CheckErrorCode(errorCode, "Subscribe");
            }
        }

        Connected = errorCode == 0;
        Logger.LogMessage(errorCode == 0 ? LogImportance.Information : LogImportance.Warn, errorCode == 0 ? $"Connected to {deviceName}!" : $"Could not connect to {deviceName}");
    }

    private void CheckErrorCode(int errorCode, string value)
    {
        if (errorCode != 0)
        {
            Logger.LogMessage(LogImportance.Warn, $"Device: {deviceName} : ErrorCode {value} : {errorCode}");
        }
    }
    
}