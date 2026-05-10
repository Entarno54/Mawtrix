using Mawtrix.Matrix.Sdk.Core.Domain.RoomEvent;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Event;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room.State;
using ImageMessageEvent = Mawtrix.Matrix.Sdk.Core.Domain.RoomEvent.ImageMessageEvent;

namespace Mawtrix.Functions;

public static class Message
{
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
        
            return $"{Program.Profiles[senderUserId].displayname} ({senderUserId}): //SENT AN IMAGE//";
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