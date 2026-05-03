using Mawtrix.Matrix.Sdk.Core.Domain.RoomEvent;

namespace Mawtrix.Matrix.Sdk
{
    public class MatrixRoomEventsEventArgs : EventArgs
    {
        public MatrixRoomEventsEventArgs(List<BaseRoomEvent> matrixRoomEvents, string nextBatch)
        {
            MatrixRoomEvents = matrixRoomEvents;
            NextBatch = nextBatch;
        }

        public List<BaseRoomEvent> MatrixRoomEvents { get; }
        
        public string NextBatch { get; }
    }
}