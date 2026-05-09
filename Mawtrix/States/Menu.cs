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
        Console.WriteLine("Press up or down to scroll, press Space|Enter to select, ESC to exit, L to join a Public Chat by it's Alias, J to DM a user");

        ConsoleKeyInfo input = Console.ReadKey();
        switch (input.Key)
        {
            case ConsoleKey.DownArrow:
                _selected += 1;
                break;
            
            case ConsoleKey.UpArrow:
                _selected -= 1;
                break;
            
            case ConsoleKey.Spacebar or ConsoleKey.Enter:
                Program.CurrentRoom = Program.Rooms[_selected].instance.Id;
                Program.State = "chat";
                break;
            
            case  ConsoleKey.Escape:
                Program.Running = false;
                Program.Client.Stop();
                break;
            
            case ConsoleKey.D:
                Program.State = "direct";
                break;
            
            case ConsoleKey.J:
                Program.State = "join";
                break;
            
            case ConsoleKey.L:
                await Program.Client.LeaveRoomAsync(Program.Rooms[_selected].instance.Id);
                Program.Rooms.RemoveAt(_selected);
                break;
            
        }

        _selected = Math.Clamp(_selected, 0, Program.Rooms.Count - 1);
    }
}