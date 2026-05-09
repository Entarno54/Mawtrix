namespace Mawtrix.States;

using Matrix.Sdk.Core.Domain.RoomEvent;
using Functions;


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
            await Program.Client.SendTypingSignal(Program.CurrentRoom, new TimeSpan(0, 0, 0, 10));
            await Task.Delay(5000);
            _canType = true;
        }
    }
    
    private async Task HandleInput()
    {
        while (true)
        {
            var key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    if (string.IsNullOrWhiteSpace(_currentInput)) break;
                    
                    await Program.Client.SendMessageAsync(Program.CurrentRoom, _currentInput);
                    _currentInput = "";
                    RenderChat();
                    break;
                
                case ConsoleKey.Backspace:
                    if(_currentInput.Length == 0) break;
                    
                    _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                    RenderChat();
                    break;
                
                case ConsoleKey.Escape:
                    Program.CurrentRoom = "";
                    Program.State = "menu";
                    return;
                
                default:
                    if (char.IsControl(key.KeyChar)) break;
                    
                    _ = SendType();
                    _currentInput += key.KeyChar;
                    RenderChat();
                    break;
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
                if (preloadedMessages > Console.WindowHeight - 5)
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

            string? toMessage = Message.EventToMessage(state);

            if (!string.IsNullOrEmpty(toMessage))
            {
                Add(toMessage);
            }
        }
    }

    public void Add(string message)
    {
        _messageList.Add(message);
    }
}