import * as signalR from "@microsoft/signalr";

const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5001/notificationHub", {
      withCredentials: true,
      transport: signalR.HttpTransportType.WebSockets
  })
  .configureLogging(signalR.LogLevel.Debug)
  .withAutomaticReconnect()
  .build();

export default hubConnection;