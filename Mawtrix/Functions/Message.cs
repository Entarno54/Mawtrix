using Meowtrix.Sdk.Core.Domain.RoomEvent;
using SixLabors.ImageSharp;
using ImageMessageEvent = Meowtrix.Sdk.Core.Domain.RoomEvent.ImageMessageEvent;
using SixLabors.ImageSharp.Processing;
using SixPix;
using Image = SixLabors.ImageSharp.Image;

namespace Mawtrix.Functions;

public static class Message
{
    private static readonly float TargetHeight = 350f;
    
    public static string? EventToMessage(BaseRoomEvent roomEvent) 
    {
        if (roomEvent is TextMessageEvent textMessageEvent)
        {
            string senderUserId = textMessageEvent.SenderUserId;
            string message = textMessageEvent.Message;
            
            if (!Program.Profiles.ContainsKey(senderUserId))
            {
                Program.Profiles[senderUserId] = Program.Client.GetUserProfile(senderUserId).Result;
            }
        
            return $"{Program.Profiles[senderUserId].displayname} ({senderUserId}): {message}";
        }

        if (roomEvent is ImageMessageEvent imageMessageEvent)
        {
            string senderUserId = imageMessageEvent.SenderUserId;
            
            if (!Program.Profiles.ContainsKey(senderUserId))
            {
                Program.Profiles[senderUserId] = Program.Client.GetUserProfile(senderUserId).Result;
            }

            Uri uri = new Uri(imageMessageEvent.MxcUrl);

            Uri newUrl =
                new Uri(Program.Client.BaseAddress + "_matrix/client/v1/media/download/" + uri.Host + uri.PathAndQuery);
            
            byte[] imageBytes = Program.HttpClient.GetByteArrayAsync(newUrl).Result;
            
            File.WriteAllBytes(imageMessageEvent.EventId+".png", imageBytes);
            
            var fileStream = new FileStream(imageMessageEvent.EventId+".png", FileMode.Open);
    
            Image img = Image.Load(imageMessageEvent.EventId+".png");

            //I don't even know how I coded ts, it limits images to 500 pixels in height
            if (img.Height > TargetHeight)
            {
                float aspect = TargetHeight / img.Height;
                
                img.Mutate(x =>  x.Resize(0, (int)Math.Floor(img.Height * aspect), KnownResamplers.Lanczos3));
                
                img.SaveAsPng("resized"+imageMessageEvent.EventId+".png");
                
                fileStream = new FileStream("resized"+imageMessageEvent.EventId+".png", FileMode.Open);
            }
            
            ReadOnlySpan<char> sixelString = Sixel.Encode(fileStream);
            // Console.Out.WriteLine(sixelString);

            return $"{Program.Profiles[senderUserId].displayname} ({senderUserId}): //SENT AN IMAGE// \n{sixelString}";
        }

        if (roomEvent is MembershipEvent inviteEvent)
        {
            string senderUserId = inviteEvent.SenderUserId;
            
            //idk where this is for now so i'll just leave it like that
            //string invitedUserId = inviteEvent.
            
            if (!Program.Profiles.ContainsKey(senderUserId))
            {
                Program.Profiles[senderUserId] = Program.Client.GetUserProfile(senderUserId).Result;
            }
        
            return $"{Program.Profiles[senderUserId].displayname} ({senderUserId}): //INVITED//";
        }

        return null;
    }
}