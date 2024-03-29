using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ClientApplication.Bike;
using ClientApplication.Util;
using ClientApplication.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ClientApplication.ServerConnection;

public class Client : DefaultClientConnection
{
    private Dictionary<string, ICommandHandler> commandHandler = new();
    private string currentBikeRecording = "";
    public Client()
    {
        Logger.LogMessage(LogImportance.Information, "Connection with Server started");
        commandHandler.Add("public-rsa-key", new RsaKey());
        
        Init(Shared.ServerConnection.Hostname, Shared.ServerConnection.Port, (json, encrypted) =>
        {
            string extraText = encrypted ? "Encrypted " : "";
            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got {extraText}message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                commandHandler[json["id"]!.ToObject<string>()!].HandleCommand(this, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got {extraText}message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        });
        if (Connected)
        {
            commandHandler.Add("encryptedMessage", new EncryptedMessage(Rsa));
            commandHandler.Add("forward-set-resistance", new SetResistance());
            commandHandler.Add("forward-chat-message", new ChatMessage());
            commandHandler.Add("start-bike-recording", new StartBikeRecording());
            commandHandler.Add("stop-bike-recording", new StopBikeRecording());
            Thread.Sleep(500);
        }

        BikeHandler handler = App.GetBikeHandlerInstance();
        handler.Subscribe(DataType.Distance, value =>
        {
            SendValue("distance", Math.Round(value,2));
        });
        handler.Subscribe(DataType.Speed, value =>
        {
            SendValue("speed", Math.Round(value,2));
        });
        handler.Subscribe(DataType.HeartRate, value =>
        {
            SendValue("heartrate", Math.Round(value,2));
        });
        
    }

    /// <summary>
    /// This function sets the current bike recording to the uuid passed in
    /// </summary>
    /// <param name="uuid">The unique identifier for the recording.</param>
    public void SetCurrentBikeRecording(string uuid)
    {
        currentBikeRecording = uuid;
        App.GetBikeHandlerInstance().Bike.Reset();
    }

    /// <summary>
    /// It sends a value to the server
    /// </summary>
    /// <param name="type">The type of data you want to send. This can be any of the following:</param>
    /// <param name="val">The value to send</param>
    /// <returns>
    /// A JSON object
    /// </returns>
    public void SendValue(string type, double val)
    {
        if (currentBikeRecording.Length <= 3)
            return;
        var serial = Shared.Util.RandomString();
        JObject ob = JsonFileReader.GetObject("ChangeData", new Dictionary<string, string>()
        {
            {"_uuid_", currentBikeRecording},
            {"_serial_", serial}
        }, JsonFolder.ServerConnection.Path);
        JObject newData = new JObject();
        newData.Add("time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
        newData.Add("value", val.ToString(CultureInfo.InvariantCulture));
        JArray value = (JArray) ob["data"]![type]!;
        value.Add(newData);
        ob["data"]![type] = value;
        SendEncryptedData(ob.ToString());
        AddSerialCallback(serial, ob =>
        {
            if (!ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
            {
                currentBikeRecording = "";
            }
        });
    }

    /// <summary>
    /// If the connection is closed, shut down the application
    /// </summary>
    public override void OnDisconnect()
    {
        Logger.LogMessage(LogImportance.Fatal, "Connection with server Closed. Shutting down.");
        Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate()
        {
            Environment.Exit(0);
            Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Invalid);
        });
        
        App.CurrentDispatcher.BeginInvoke((Action)delegate()
        {
            Application.Current.Shutdown(500);
            System.Environment.Exit(500);
            Environment.Exit(0);
            App.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Invalid);
        });
    }
    
    public override void OnNotConnected()
    {
        App.CurrentDispatcher.Invoke(() =>
        {
            Logger.LogMessage(LogImportance.Debug, "Not connected");
            LoginViewModel.Model.ErrorMessage = "Could not connect with server.";
        });
    }
}