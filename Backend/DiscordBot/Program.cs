
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

Uri uri = new("ws://gateway.discord.gg");
string token = "";
int interval = 0;

using SocketsHttpHandler handler = new();
using ClientWebSocket ws1 = new();
await ws1.ConnectAsync(uri, new HttpMessageInvoker(handler), default);
var source = new CancellationTokenSource();


var initial_payload = new Payload
{
    op = 2,
    d = new Data
    {
        token = token,
        intents = 513,
        properties = new Properties
        {
            os = "Windows",
            browser = "chrome",
            device = "chrome"
        }
    }
};



ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(initial_payload)));
await ws1.SendAsync(bytesToSend, WebSocketMessageType.Text, true, source.Token);


bool heartbeatFlag = false;

var heartbeatThread = new Thread(() =>
{
    while(true)
    {
        Console.WriteLine($"Sent heartbeat");
        Thread.CurrentThread.IsBackground = true;
        System.Threading.Thread.Sleep(interval);
        ArraySegment<byte> heartbeat = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Payload { op = 1, d = null })));
        ws1.SendAsync(heartbeat, WebSocketMessageType.Text, true, CancellationToken.None);
        
    }
});


while (ws1.State == WebSocketState.Open)
{
    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4096]);
    WebSocketReceiveResult response = await ws1.ReceiveAsync(buffer, source.Token);

    if (response.MessageType == WebSocketMessageType.Text)
    {
        string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, response.Count);
        Console.WriteLine($"Unrefined: {receivedMessage}\n\n\n");
        try
        {
            
            Message gatewayEvent = JsonSerializer.Deserialize<Message>(receivedMessage);

            if (gatewayEvent.op == 10 && gatewayEvent.d.heartbeat_interval != null)
            {
                
                interval = (int)gatewayEvent.d.heartbeat_interval;
                heartbeatThread.Start();
                
            }



            if (gatewayEvent.t != null && gatewayEvent.t.Equals("MESSAGE_CREATE")){
                Console.WriteLine($"Received message from discord: {gatewayEvent.d.content}");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.Message);
        }


        

    }
    else if (response.MessageType == WebSocketMessageType.Close)
    {
        await ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, "", source.Token);
    }
}







public class Message
{
    public int op { get; set; }
    public string? t { get; set; }
    public int? s { get; set; }
    
    public MessageData? d { get; set; }
}


public class MessageData
{
    public int type { get; set; }
    public bool tts { get; set; }
    public DateTime timestamp { get; set; }
    public string id { get; set; }
    public int flags { get; set; }
    public object edited_timestamp { get; set; }
    public string content { get; set; }
    public Author author { get; set; }
    public int? heartbeat_interval { get; set; }

}

public class Author
{
    public string username { get; set; }
    public string id { get; set; }
    public string global_name { get; set; }

}




public class Payload
{
    public int op { get; set; }
    public Data? d { get; set; }
}

public class Data
{
    public string token { get; set; }
    public int intents { get; set; }
    public Properties properties { get; set; }
}

public class Properties
{
    public string os { get; set; }
    public string browser { get; set; }
    public string device { get; set; }
}