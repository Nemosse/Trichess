using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    private HttpListener httpListener;
    private Thread listenerThread;

    void Start()
    {
        StartServer();
    }

    private void StartServer()
    {
        httpListener = new HttpListener();

        // Dynamically get the machine's IP addresses
        IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

        foreach (IPAddress ipAddress in localIPs)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork) // IPv4 addresses
            {
                string prefix = $"http://{ipAddress}:8080/";
                httpListener.Prefixes.Add(prefix);
            }
        }

        listenerThread = new Thread(new ThreadStart(ListenForClients));
        listenerThread.Start();
    }

    private void ListenForClients()
    {
        httpListener.Start();

        while (true)
        {
            try
            {
                HttpListenerContext context = httpListener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception e)
            {
                Debug.LogError("Error handling request: " + e.Message);
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        // Assume the incoming data is JSON
        string json = "";
        using (Stream body = request.InputStream)
        {
            using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
            {
                json = reader.ReadToEnd();
            }
        }

        // Process the JSON data
        string jsonResponse = ProcessJsonRequest(json);

        byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }

    private string ProcessJsonRequest(string jsonRequest)
    {
        // Process the JSON request and return a JSON response
        // You need to implement your own logic here
        Debug.Log("Received JSON: " + jsonRequest);

        // For now, just return a simple JSON response
        return "{ \"message\": \"Hello, client!\" }";
    }

    void OnDestroy()
    {
        httpListener.Stop();
        listenerThread.Abort();
    }
}