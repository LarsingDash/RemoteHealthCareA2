using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;
using ClientApplication.ServerConnection.Communication.CommandHandlers;
using DoctorApplication.Communication.CommandHandlers;
using DoctorApplication.ViewModel;
using Shared;
using Shared.Log;
using Formatting = Newtonsoft.Json.Formatting;

namespace DoctorApplication.Communication;

public class Client : DefaultClientConnection
{
    private Dictionary<string, ICommandHandler> commandHandler = new();
    public List<string> hideMessages = new List<string>();
    
    public Client()
    {
        commandHandler.Add("public-rsa-key", new RsaKey());
        hideMessages = new List<string>()
        {
        };
        
        Init(ServerConnection.Hostname, ServerConnection.Port, (json, encrypted) =>
            {
                string extraText = encrypted ? "Encrypted " : "";
               if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
               {
                   if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage") && !hideMessages.Contains(json["id"]!.ToObject<string>()!))
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
        
        commandHandler.Add("encryptedMessage", new EncryptedMessage(Rsa));
        commandHandler.Add("user-state-changed", new UserStateChange());
        commandHandler.Add("update-values", new UpdateValues());

        Thread.Sleep(500);
    }
    
    /// <summary>
    /// > If the connection with the server is closed, then shut down the application
    /// </summary>
    public override void OnDisconnect()
    {
        Logger.LogMessage(LogImportance.Fatal, "Connection with server Closed. Shutting down.");
        Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate()
        {
            // Application.Current.Shutdown(500);
            // System.Environment.Exit(500);
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