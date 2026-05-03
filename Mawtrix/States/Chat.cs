using Mawtrix.Matrix.Sdk.Core.Domain.RoomEvent;

namespace Mawtrix.States;

public class Chat
{
    private string _currentInput = "";
    private List<string> _messageList = new();
    private bool _canType = true;
    
    public void RenderChat()
    {
        Console.Clear();
        int messagesToShow = Math.Min(_messageList.Count, Console.WindowHeight - 3);
        for (int i = _messageList.Count - messagesToShow; i < _messageList.Count; i++)
        {
            Console.WriteLine(_messageList[i]);
        }

        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write("> " + _currentInput);
    }

    private async Task SendType()
    {
        if (_canType)
        {
            _canType = false;
            _ = Program.Client.SendTypingSignal(Program.CurrentRoom, new TimeSpan(0, 0, 0, 10));
            await Task.Delay(11000);
            _canType = true;
        }
    }
    
    private async Task HandleInput()
    {
        while (true)
        {
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
            {
                if (!string.IsNullOrWhiteSpace(_currentInput))
                {
                    await Program.Client.SendMessageAsync(Program.CurrentRoom, _currentInput);
                    _currentInput = "";
                    RenderChat();
                }
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                Program.CurrentRoom = "";
                Program.State = "menu";
                break;
            }
            else if (key.Key == ConsoleKey.Backspace && _currentInput.Length > 0)
            {
                _ = SendType();
                _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                RenderChat();
            }
            else if (!char.IsControl(key.KeyChar))
            {
                _currentInput += key.KeyChar;
                RenderChat();
            }
        }
    }
    
    public async Task Update()
    {
        var inputTask = HandleInput();
        
        int lastMessageCount = 0;
        while (Program.CurrentRoom != "" && Program.State == "chat")
        {
            if (_messageList.Count != lastMessageCount)
            {
                lastMessageCount = _messageList.Count;
                RenderChat();
            }
        
            await Task.Delay(100);
        }
        
        await inputTask;
    }

    public void Load()
    {
        _currentInput = "";
        _messageList = new();
    }

    public void Messages()
    {
        int preloadedMessages = 0;

        Task<bool> StopCallback(BaseRoomEvent a)
        {
            try
            {
                preloadedMessages += 1;
                if (preloadedMessages > 30)
                {
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception exception)
            {
                return Task.FromException<bool>(exception);
            }
        }

        var history = Program.Client.GetHistory(Program.CurrentRoom, StopCallback).Result;
        
        for (var i = history.Count - 1;i>=0; i--)
        {
            var state = history[i];
            
            if (state is not TextMessageEvent messageEvent) continue;
            string senderUserId = messageEvent.SenderUserId;
            string message = messageEvent.Message;
            if (!Program.Profiles.ContainsKey(senderUserId))
            {
                //Console.WriteLine("Loading profile "+senderUserId);
                Program.Profiles[senderUserId] = Program.Client.GetUserProfile(senderUserId).Result;
            }
            //Console.WriteLine("Message added");
            Add($"{Program.Profiles[senderUserId].displayname} ({senderUserId}): {message}");
            //_ = Update();
        }
    }

    public void Add(string message)
    {
        _messageList.Add(message);
    }
}