using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync.Event;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room.State;

namespace Mawtrix.Matrix.Sdk.Core.Domain.RoomEvent
{
    public record InviteToRoomEvent(string EventId, string RoomId, string SenderUserId, DateTimeOffset Timestamp) : BaseRoomEvent(EventId, RoomId, SenderUserId, Timestamp)
    {
        public static class Factory
        {
            public static bool TryCreateFrom(RoomEventResponse roomEvent, string roomId,
                out InviteToRoomEvent inviteToRoomEvent)
            {
                RoomMemberContent content = roomEvent.Content.ToObject<RoomMemberContent>();
                if (roomEvent.EventType == EventType.Member &&
                    content?.UserMembershipState == UserMembershipState.Invite)
                {
                    inviteToRoomEvent = new InviteToRoomEvent(roomEvent.EventId, roomId, roomEvent.SenderUserId, roomEvent.Timestamp);
                    return true;
                }

                inviteToRoomEvent = null;
                return false;
            }

            public static bool TryCreateFromStrippedState(RoomStrippedState roomStrippedState, string roomId,
                out InviteToRoomEvent inviteToRoomEvent)
            {
                RoomMemberContent content = roomStrippedState.Content.ToObject<RoomMemberContent>();
                if (roomStrippedState.EventType == EventType.Member &&
                    content?.UserMembershipState == UserMembershipState.Invite)
                {
                    inviteToRoomEvent = new InviteToRoomEvent(string.Empty, roomId, roomStrippedState.SenderUserId, DateTimeOffset.MinValue);
                    return true;
                }

                inviteToRoomEvent = null;
                return false;
            }
        }
    }
}