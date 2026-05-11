namespace Mawtrix.States;

using Meowtrix.Sdk.Core.Domain.MatrixRoom;

public class Join
{
    private static string _currentInput = "";
    
    public async Task Update()
    {
        Console.Clear();
        
        Console.WriteLine("Put in the ID of the user you want to make a chat with:");
        Console.Write(_currentInput);

        ConsoleKeyInfo input = Console.ReadKey(true);

        switch (input.Key)
        {
            case ConsoleKey.Enter:
                try
                {
                    string room = await Program.Client.GetPublicRoomIdFromAlias(_currentInput);

                    await Program.Client.JoinTrustedPrivateRoomAsync(room);

                    MatrixRoom newRoom = new MatrixRoom(room, MatrixRoomStatus.Joined, new List<string>());

                    Program.Rooms.Add(new
                    {
                        instance = newRoom, name = _currentInput
                    });

                    Program.CurrentRoom = room;
                    Program.State = "chat";
                }
                catch (HttpRequestException exception)
                {
                    Console.WriteLine(exception.Message);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                break;
            
            case ConsoleKey.Backspace:
                _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                break;
            
            case ConsoleKey.Escape:
                Program.State = "menu";
                break;
            
            default:
                if (char.IsControl(input.KeyChar)) break;
                _currentInput += input.KeyChar;
                break;
        }
    }

    public void Load()
    {
        _currentInput = "";
    }
}