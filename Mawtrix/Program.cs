namespace Mawtrix;

using Matrix.Sdk;
using System.Text.Json;
using Matrix.Sdk.Core.Domain.RoomEvent;
using Matrix.Sdk.Core.Infrastructure.Extensions;
using Matrix.Sdk.Core.Infrastructure.Dto.User;
using States;
using Functions;
using DeviceId;


static class Program
{
    private static readonly MatrixClientFactory Factory = new MatrixClientFactory();
    public static readonly IMatrixClient Client = Factory.Create();
    public static string State = "menu";
    public static List<dynamic> Rooms = new();
    public static string CurrentRoom = "";
    
    public static readonly Dictionary<string, MatrixProfile> Profiles = new ();

    public static bool Running = true;

    private static readonly Menu MenuState = new Menu();
    private static readonly Chat ChatState = new Chat();
    private static readonly Direct DirectState = new Direct();
    private static readonly Join JoinState = new Join();
    
    static async Task Main()
    {
        Console.WriteLine("Hello");
        Console.WriteLine("Put in your matrix homeserver url: \nTo use previous credentials, type in 'login':");
        string? federationTest = Console.ReadLine();

        Uri homeserver;
        string? login;
        string? password = "";

        HttpClient httpClient = new HttpClient();

        if (federationTest == "login")
        {
            var data = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "/Data");

            var decrypted = Encryption.Decrypt(data, EncKey.Key, EncKey.Iv);

            var realData = JsonDocument.Parse(decrypted).RootElement;
            homeserver = new(realData.GetProperty("homeserver").GetString() ?? "matrix.org");
            login = realData.GetProperty("login").GetString();
            password = realData.GetProperty("password").GetString();
        }
        else
        {
            //var data = new { homeserver = new Uri("https://matrix.org/"), login="test", password="test"};
            try
            {
                string responseBody =
                    await httpClient.GetStringAsync("https://" + federationTest + "/_matrix/federation/v1/version");
                Console.WriteLine(responseBody);

                homeserver = new("https://" + federationTest);
            }
            catch (HttpRequestException)
            {
                try
                {
                    string responseBody =
                        await httpClient.GetStringAsync("https://" + federationTest + "/.well-known/matrix/server");
                    Console.WriteLine(responseBody);

                    var parsed = JsonDocument.Parse(responseBody);

                    parsed.RootElement.TryGetProperty("m.server", out var server);
                    if (server.GetString() == null) return;

                    // Still in the same try scope so if it errors out it'll catch the exception too
                    await httpClient.GetStringAsync("https://" + server.GetString());

                    homeserver = new("https://" + server.GetString());
                }
                catch (HttpRequestException)
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
                }
                else if (key.Key == ConsoleKey.Backspace)
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


            var data = new { homeserver, password, login };

            var ser = JsonSerializer.Serialize(data);

            var encrypted = Encryption.Encrypt(ser, EncKey.Key, EncKey.Iv);

            await File.WriteAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "/Data", encrypted);
        }

        if (login != null && password != null)
        {
            string deviceId = new DeviceIdBuilder().AddMachineName().ToString();
            
            await Client.LoginAsync(homeserver, login, password, "SillyLittleThing");
            

            Client.OnMatrixRoomEventsReceived += (_, eventArgs) =>
            {
                foreach (BaseRoomEvent roomEvent in eventArgs.MatrixRoomEvents)
                {
                    if (roomEvent.RoomId == CurrentRoom)
                    {
                        string? message = Message.EventToMessage(roomEvent);
                        if (!string.IsNullOrEmpty(message))
                        {
                            ChatState.Add(message);
                        }
                    } else if (roomEvent is MembershipEvent membershipEvent)
                    {
                    }
                }
            };
            Console.WriteLine("Successfully logged in as " + login);
        }

        Client.Start();

//Waiting for the rooms to load
        while (Client.JoinedRooms.Length == 0)
        {
            await Task.Delay(100);
            Console.WriteLine("Still no rooms.");
        }

        if (Client.Token != null) httpClient.AddBearerToken(Client.Token);
        foreach (var room in Client.JoinedRooms)
        {
            Console.WriteLine(room);
            Console.WriteLine(homeserver + "_matrix/client/v3/rooms/" + room.Id + "/state");
            string a = await httpClient.GetStringAsync(homeserver + "_matrix/client/v3/rooms/" + room.Id + "/state");
            JsonElement states = JsonDocument.Parse(a).RootElement;
            //Console.WriteLine(states);
            //Console.WriteLine(parsed.RootElement.GetProperty("name"));
            //Console.WriteLine(a);
            string? roomName;
            try
            {
                string b = await httpClient.GetStringAsync(homeserver + "_matrix/client/v3/rooms/" + room.Id +
                                                           "/state/m.room.name");
                //Console.WriteLine(a);
                roomName = JsonDocument.Parse(b).RootElement.GetProperty("name").ToString();
            }
            catch (HttpRequestException)
            {
                roomName = "";
                foreach (JsonElement roomState in states.EnumerateArray())
                {
                    if (roomState.GetProperty("type").GetString() == "m.room.member")
                    {
                        string? userid = roomState.GetProperty("sender").GetString();
                        if (userid == Client.UserId ||
                            roomState.GetProperty("content").GetProperty("membership").ToString() != "join") continue;
                        roomName += userid + " ";
                    }
                }
            }

            Rooms.Add(new { instance = room, name = roomName });
        }

        Rooms = Rooms.OrderBy(f => f.name).ToList();
        

        string lastState = "menu";
        while (Running)
        {
            if (lastState != State)
            {
                lastState = State;
                if (State == "chat")
                {
                    ChatState.Load();
                    _ = Task.Run(() => ChatState.Messages());
                    ChatState.RenderChat();
                } else if (State == "direct")
                {
                    DirectState.Load();
                }else if (State == "join")
                {
                    JoinState.Load();
                }
            } 
            
            if (State == "chat")
            {
                await ChatState.Update();
            } else if (State == "menu")
            {
                await MenuState.Update();
            } else if (State == "direct")
            {
                await DirectState.Update();
            } else if (State == "join")
            {
                await JoinState.Update();
            }
        }
    }
}