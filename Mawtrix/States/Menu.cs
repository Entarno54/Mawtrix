namespace Mawtrix.States;

public class Menu
{
    private static int _selected;
    
    public async Task Update()
    {
        Console.Clear();
        int cur = 0;
        
        foreach (var room in Program.Rooms)
        {
            //Console.WriteLine(cur);
            if (cur == _selected)
            {
                Console.WriteLine(">" + room.name);
            }
            else
            {
                Console.WriteLine(room.name);
            }

            cur += 1;
        }
        Console.WriteLine("Press up or down to scroll, press Space to select, ESC to exit, L to join a Public Chat by it's Alias, J to DM a user");

        ConsoleKeyInfo input = Console.ReadKey();
        if (input.Key == ConsoleKey.DownArrow)
        {
            _selected += 1;
        }
        else if (input.Key == ConsoleKey.UpArrow)
        {
            _selected -= 1;
        }
        else if (input.Key == ConsoleKey.Spacebar)
        {
            Program.CurrentRoom = Program.Rooms[_selected].instance.Id;
            Program.State = "chat";
        }
        else if (input.Key == ConsoleKey.Escape)
        {
            Program.Running = false;
            Program.Client.Stop();
        } else if (input.Key == ConsoleKey.D)
        {
            Program.State = "direct";
        } else if (input.Key == ConsoleKey.J)
        {
            Program.State = "join";
        } else if (input.Key == ConsoleKey.L)
        {
            await Program.Client.LeaveRoomAsync(Program.Rooms[_selected].instance.Id);
            Program.Rooms.RemoveAt(_selected);
        }

        _selected = Math.Clamp(_selected, 0, Program.Rooms.Count - 1);
    }
}