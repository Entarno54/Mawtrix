namespace Mawtrix.States;

using System.Net;
using System.Text.Json;
using Mawtrix.Matrix.Sdk.Core.Domain.MatrixRoom;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync;
using Matrix.Sdk.Core.Infrastructure.Dto.Room.Join;

public class Join
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
                //Program.Client.
                string room = await Program.Client.GetPublicRoomIdFromAlias(_currentInput);

                await Program.Client.JoinTrustedPrivateRoomAsync(room);

                MatrixRoom newRoom = new MatrixRoom(room, MatrixRoomStatus.Joined, new List<string>());
                
                Program.Rooms.Add(new {
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