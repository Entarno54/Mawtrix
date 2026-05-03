namespace Mawtrix.States;

public class Menu
{
    private static int _selected = 0;
    
    public void Update()
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
            return;
        }

        _selected = Math.Clamp(_selected, 0, Program.Rooms.Count - 1);
    }
}