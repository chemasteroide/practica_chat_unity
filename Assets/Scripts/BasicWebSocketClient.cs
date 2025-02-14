using UnityEngine;
using WebSocketSharp;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;



public class BasicWebSocketClient : MonoBehaviour
{
    // Instancia del cliente WebSocket
    private WebSocket ws;

    public TMP_Text chatdisplay;
    public TMP_InputField textoMensaje;
    public ScrollRect scrollRect;

    private bool connected;

    private Queue<Action> actionsToRun = new Queue<Action>();

    // Se ejecuta al iniciar la escena
    void Start()
    {
        chatdisplay.text = "";
        textoMensaje.onSubmit.AddListener(delegate { SendMessageToServer(); });
    }

    void Update()
    {
        if (!connected)
        {
            try
            {
                // Crear una instancia del WebSocket apuntando a la URI del servidor
                ws = new WebSocket("ws://127.0.0.1:7777/");

                // Evento OnOpen: se invoca cuando se establece la conexión con el servidor
                ws.OnOpen += (sender, e) =>
                {
                    Debug.Log("WebSocket conectado correctamente.");
                };

                // Evento OnMessage: se invoca cuando se recibe un mensaje del servidor
                ws.OnMessage += (sender, e) =>
                {
                    EnqueueUIAction(() => {chatdisplay.text += e.Data + '\n';});
                    EnqueueUIAction(() => {Canvas.ForceUpdateCanvases();
                                           scrollRect.verticalNormalizedPosition = 0f;});
                    //EnqueueUIAction(() => {Actualizar(e.Data, chatdisplay);});
                    
                    Debug.Log("Mensaje recibido: " + e.Data);
                };

                // Evento OnError: se invoca cuando ocurre un error en la conexión
                ws.OnError += (sender, e) =>
                {
                    Debug.LogError("Error en el WebSocket: " + e.Message);
                };

                // Evento OnClose: se invoca cuando se cierra la conexión con el servidor
                ws.OnClose += (sender, e) =>
                {
                    Debug.Log("WebSocket cerrado. Código: " + e.Code + ", Razón: " + e.Reason);
                };

                // Conectar de forma asíncrona al servidor WebSocket
                ws.ConnectAsync();

                connected = true;
            } 
            catch (Exception e)
            {
                print(e.Message);
            }


            
        }


        if (actionsToRun.Count > 0)
        {
            Action action;

            lock (actionsToRun)
            {
                action = actionsToRun.Dequeue();
            }

            action?.Invoke();            
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {

        }        
    }

        void EnqueueUIAction(Action action)
    {
        lock (actionsToRun)
        {
            actionsToRun.Enqueue(action);
        }
    }

    // Método para enviar un mensaje al servidor (puedes llamarlo, por ejemplo, desde un botón en la UI)
    public void SendMessageToServer()
    {
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            if (!String.IsNullOrEmpty(textoMensaje.text))
            {
                ws.Send(textoMensaje.text);
                textoMensaje.text = "";
                textoMensaje.ActivateInputField();
            }
            else
            {
                Debug.LogError("El mensaje es nulo o está vacío");
            }
        }
        else
        {
            Debug.LogError("No se puede enviar el mensaje. La conexión no está abierta.");
        }
    }

    // Se ejecuta cuando el objeto se destruye (por ejemplo, al cambiar de escena o cerrar la aplicación)
    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }


}
