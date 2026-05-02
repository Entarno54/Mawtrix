using System.Security.Cryptography;
using Mawtrix.Matrix.Sdk;
using System.Text.Json;
using Mawtrix;
using Mawtrix.Matrix.Sdk.Core.Domain.RoomEvent;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Extensions;
using static Mawtrix.Encryption;

var factory = new MatrixClientFactory();
IMatrixClient client = factory.Create();

Console.WriteLine("Hello");
Console.WriteLine("Put in your matrix homeserver url: \nTo use previous credentials, type in 'login':");
string? federationtest = Console.ReadLine();

Uri homeserver;
string? login;
string password = "";

string state = "menu";
string currentRoom = "";
List<string> messageList = new();

HttpClient httpClient = new HttpClient();

if (federationtest == "login")
{
    var data = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Data");

    var decrypted = Decrypt(data, Key.key, Key.iv);
    
    var realData = JsonDocument.Parse(decrypted).RootElement;
    homeserver = new(realData.GetProperty("homeserver").GetString());
    login = realData.GetProperty("login").GetString();
    // VERY UNSAFE BUT IDK HOW TO DO IT LITEARLLY
    password = realData.GetProperty("password").GetString();
}
else
{
    //var data = new { homeserver = new Uri("https://matrix.org/"), login="test", password="test"};
    try
    {
        string responseBody = await httpClient.GetStringAsync("https://"+federationtest+"/_matrix/federation/v1/version");
        Console.WriteLine(responseBody);
    
        homeserver = new("https://" + federationtest);
    }
    catch (HttpRequestException e)
    {
        try
        {
            string responseBody =
                await httpClient.GetStringAsync("https://" + federationtest + "/.well-known/matrix/server");
            Console.WriteLine(responseBody);

            var parsed = JsonDocument.Parse(responseBody);

            parsed.RootElement.TryGetProperty("m.server", out var server);
            if (server.GetString() == null) return;
            
            // Still in the same try scope so if it errors out it'll catch the exception too
            await httpClient.GetStringAsync("https://" + server.GetString());
        
            homeserver = new("https://" + server.GetString());

        }
        catch (HttpRequestException a)
        {
            Console.WriteLine("Homeserver invalid!");
            return;
        }
    }

    Console.WriteLine(homeserver + " is a valid homeserver!");

    Console.WriteLine("Put in your matrix login:");
    login = Console.ReadLine();

    if (string.IsNullOrEmpty(login)) return;

    Console.WriteLine("Put in your matrix password:");

    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);


        if (key.Key == ConsoleKey.Enter)
        {
            break;
        } else if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password = password[..^1];
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Console.Write(" ");
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password += key.KeyChar;
            Console.Write("*");
        }
    }
    

    if (string.IsNullOrEmpty(password)) return;


    var data = new { homeserver = homeserver, password = password, login = login };

    var ser = JsonSerializer.Serialize(data);

    var encrypted = Encrypt(ser, Key.key, Key.iv);
    
    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/Data", encrypted);
}

await client.LoginAsync(homeserver, login, password, "SillyLittleBoy");

client.OnMatrixRoomEventsReceived += (sender, eventArgs) =>
{
    foreach (BaseRoomEvent roomEvent in eventArgs.MatrixRoomEvents)
    {
        //Console.WriteLine($"Type: {roomEvent.GetType().Name}");
        if (roomEvent is MembershipEvent)
        {
            //Console.WriteLine(client.GetRoomName(roomEvent.RoomId));
        }
        if (roomEvent is not TextMessageEvent textMessageEvent)
        {
           // Console.WriteLine("Not text event");
            continue;
        }
        
        string roomId = textMessageEvent.RoomId;
        string senderUserId = textMessageEvent.SenderUserId;
        string message = textMessageEvent.Message;

        if (currentRoom == roomId)
        {
            //Console.WriteLine($"RoomId: {roomId} received message from {senderUserId}: {message}.");
            messageList.Add(senderUserId + ": " + message);
        }
    }
};
Console.WriteLine("Successfully logged in as "+login);

client.Start();

List<dynamic> rooms = new();

//Waiting for the rooms to load
while (client.JoinedRooms.Length == 0)
{
    await Task.Delay(100);
    Console.WriteLine("Still no rooms.");
}

httpClient.AddBearerToken(client.Token);
foreach (var room in client.JoinedRooms)
{
    Console.WriteLine(room);
    Console.WriteLine(homeserver + "_matrix/client/v3/rooms/" + room.Id + "/state");
    string a = await httpClient.GetStringAsync( homeserver + "_matrix/client/v3/rooms/" + room.Id + "/state" );
    var parsed = JsonDocument.Parse(a);
    //Console.WriteLine(parsed.RootElement.GetProperty("name"));
    //Console.WriteLine(a);
    string? roomname = null;
    try
    {
        string b = await httpClient.GetStringAsync( homeserver + "_matrix/client/v3/rooms/" + room.Id + "/state/m.room.name" );
        Console.WriteLine("FOUNDNAMEFOUNDNAME\n\n\n\n");
        Console.WriteLine(a);
        roomname = JsonDocument.Parse(b).RootElement.GetProperty("name").ToString();
    } catch (HttpRequestException) {}
    rooms.Add(new {instance = room, name = roomname ?? room.Id});
}

int selected = 0;

string currentInput = "";

void RenderChat()
{
    Console.Clear();
    int messagesToShow = Math.Min(messageList.Count, Console.WindowHeight - 3);
    for (int i = messageList.Count - messagesToShow; i < messageList.Count; i++)
    {
        Console.WriteLine(messageList[i]);
    }
    
    Console.SetCursorPosition(0, Console.WindowHeight - 1);
    Console.Write(new string(' ', Console.WindowWidth - 1));
    Console.SetCursorPosition(0, Console.WindowHeight - 1);
    Console.Write("> " + currentInput);
}


async Task handleInput()
{
    while (true)
    {
        var key = Console.ReadKey(true);
        
        if (key.Key == ConsoleKey.Enter)
        {
            if (!string.IsNullOrWhiteSpace(currentInput))
            {
                await client.SendMessageAsync(currentRoom, currentInput);
                currentInput = "";
                RenderChat();
            }
        }
        else if (key.Key == ConsoleKey.Escape)
        {
            currentRoom = "";
            state = "menu";
            break;
        }
        else if (key.Key == ConsoleKey.Backspace && currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            RenderChat();
        }
        else if (!char.IsControl(key.KeyChar))
        {
            currentInput += key.KeyChar;
            RenderChat();
        }
    }
}

async void chatState()
{
    while (state == "chat")
    {
        currentInput = "";
    
        var inputTask = handleInput();
    
        int lastMessageCount = 0;
        while (currentRoom != "" && state == "chat")
        {
            if (messageList.Count != lastMessageCount)
            {
                lastMessageCount = messageList.Count;
                RenderChat();
            }
            await Task.Delay(100);
        }
    
        await inputTask;
    }
}

void menuState()
{
    while (true)
    {
        Console.Clear();
        int cur = 0;
        //Console.WriteLine(cur);
        foreach (var room in rooms)
        {
            //Console.WriteLine(cur);
            if (cur == selected)
            {

                Console.WriteLine(">" + room.name);
            }
            else
            {
                Console.WriteLine(room.name);
            }

            cur += 1;
        }

        ConsoleKeyInfo input = Console.ReadKey();
        if (input.Key == ConsoleKey.DownArrow)
        {
            selected += 1;
        }
        else if (input.Key == ConsoleKey.UpArrow)
        {
            selected -= 1;
        } else if (input.Key == ConsoleKey.Spacebar)
        {
            currentRoom = rooms[selected].instance.Id;
            messageList = new();
            state = "chat";
            RenderChat();
            chatState();
        } else if (input.Key == ConsoleKey.Escape)
        {
            return;
        }

        selected = Math.Clamp(selected, 0, rooms.Count - 1);
    }
}


menuState();