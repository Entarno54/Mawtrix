using Mawtrix.Matrix.Sdk.Core.Domain.RoomEvent;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync;
using Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room;

namespace Mawtrix.Matrix.Sdk.Core.Domain.MatrixRoom
{
    public record MatrixRoom(string Id, MatrixRoomStatus Status, List<string> JoinedUserIds)
    {
        public static MatrixRoom Create(string roomId, RoomResponse joinedRoom, MatrixRoomStatus status)
        {
            var joinedUserIds = new List<string>();
            foreach (RoomEventResponse timelineEvent in joinedRoom.Timeline.Events)
                if (MembershipEvent.Factory.TryCreateFrom(timelineEvent, roomId, out MembershipEvent joinRoomEvent))
                    joinedUserIds.Add(joinRoomEvent!.SenderUserId);

            return new MatrixRoom(roomId, status, joinedUserIds);
        }

        public static MatrixRoom CreateInvite(string roomId, InvitedRoom invitedRoom)
        {
            var joinedUserIds = new List<string>();
            foreach (RoomStrippedState timelineEvent in invitedRoom.InviteState.Events)
                if (MembershipEvent.Factory.TryCreateFromStrippedState(timelineEvent, roomId,
                        out MembershipEvent joinRoomEvent))
                    joinedUserIds.Add(joinRoomEvent!.SenderUserId);

            return new MatrixRoom(roomId, MatrixRoomStatus.Invited, joinedUserIds);
        }
    }
}