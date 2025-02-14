using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System;
using TMPro;
using System.IO;

// Clase que se adjunta a un GameObject en Unity para iniciar el servidor WebSocket.
public class BasicWebSocketServer : MonoBehaviour
{
    public static string historial = "";

    // Instancia del servidor WebSocket.
    private WebSocketServer wss;

    private bool isServer;

    // Se ejecuta al iniciar la escena.
    void Start()
    {

        try
        {
            // Crear un servidor WebSocket que escucha en el puerto 7777.
            wss = new WebSocketServer(7777);

            // Añadir un servicio en la ruta "/" que utiliza el comportamiento EchoBehavior.
            wss.AddWebSocketService<EchoBehavior>("/");

            // Iniciar el servidor. 
            wss.Start();

            Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");
            isServer = true;
        } 
        catch (Exception e)
        {
            print(e.Message);
            Destroy(gameObject);
        }

    }

    // Se ejecuta cuando el objeto se destruye (por ejemplo, al cerrar la aplicación o cambiar de escena).
    void OnDestroy()
    {
        // Si el servidor está activo, se detiene de forma limpia.
        if (wss != null)
        {
            wss.Stop();
            wss = null;
            Debug.Log("Servidor WebSocket detenido.");
        }

        if (isServer)
        {
            string path = "../Historial/historial_sesión_"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ".txt";
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(historial);
            }
        }

    }

}

// Comportamiento básico del servicio WebSocket: simplemente devuelve el mensaje recibido.
public class EchoBehavior : WebSocketBehavior
{

    private static List<String> clientes = new List<String>();
    private static List<string> colorClientes = new List<string> { "orange", "red", "blue", "purple", "yellow", "green"};

    // Se invoca cuando se recibe un mensaje desde un cliente.
    protected override void OnMessage(MessageEventArgs e)
    {
        // Envía de vuelta el mismo mensaje recibido.
        int index = clientes.IndexOf(ID);
        BasicWebSocketServer.historial += "Usuario " + (index+1) + ": " + e.Data + "\n";
        Sessions.Broadcast("<color="+colorClientes[index > colorClientes.Count - 1 ? 0 : index]+">Usuario " + (index+1) + ":</color> " + e.Data);
    }

    protected override void OnOpen()
    {
        clientes.Add(ID);        
        int index = clientes.IndexOf(ID);
        BasicWebSocketServer.historial +="Usuario " + (index+1) + " se ha conectado\n";
        Sessions.Broadcast ("<color="+colorClientes[index > colorClientes.Count - 1 ? 0 : index]+">Usuario " + (index+1) + " se ha conectado</color> ");

    }

    protected override void OnClose(CloseEventArgs e)
    {
        int index = clientes.IndexOf(ID);
        BasicWebSocketServer.historial +="Usuario " + (index+1) + " se ha desconectado\n";
        Sessions.Broadcast ("<color="+colorClientes[index > colorClientes.Count - 1 ? 0 : index]+">Usuario " + (index+1) + " se ha desconectado</color> ");
    }
}
