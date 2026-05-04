namespace Mawtrix.States;

using Matrix.Sdk.Core.Domain.MatrixRoom;
using Matrix.Sdk.Core.Infrastructure.Dto.Room.Create;

public class Direct
{
    private static string _currentInput = "";
    
    public async Task Update()
    {
        Console.Clear();
        
        Console.WriteLine("Put in the ID of the user you want to make a chat with:");
        Console.Write(_currentInput);

        ConsoleKeyInfo input = Console.ReadKey(true);

        if (input.Key == ConsoleKey.Enter)
        {
            try
            {
                CreateRoomResponse response = await Program.Client.CreateTrustedPrivateRoomAsync([_currentInput]);

                MatrixRoom newRoom = new MatrixRoom(response.RoomId, MatrixRoomStatus.Joined, new List<string>());
                
                Program.Rooms.Add(new {
                    instance = newRoom, name = input
                });
                
                Program.CurrentRoom = response.RoomId;
                Program.State = "chat";
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        } else if (!char.IsControl(input.KeyChar))
        {
            _currentInput += input.KeyChar;
        } else if (input.Key == ConsoleKey.Backspace)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        } else if (input.Key == ConsoleKey.Escape)
        {
            Program.State = "menu";
        }
    }

    public void Load()
    {
        _currentInput = "";
    }
}