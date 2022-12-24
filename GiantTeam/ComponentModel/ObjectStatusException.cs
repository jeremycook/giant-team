using GiantTeam.ComponentModel.Models;
using System.Runtime.Serialization;

namespace GiantTeam.ComponentModel
{
    public class ObjectStatusException : Exception
    {
        public ObjectStatusException(ObjectStatus objectStatus) : base(objectStatus.Message)
        {
            ObjectStatus = objectStatus;
        }

        public ObjectStatusException(ObjectStatus objectStatus, Exception? innerException) : base(objectStatus.Message, innerException)
        {
            ObjectStatus = objectStatus;
        }

        protected ObjectStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ObjectStatus = (ObjectStatus)info.GetValue(nameof(ObjectStatus), typeof(ObjectStatus))!;
        }

        public ObjectStatus ObjectStatus { get; }
    }
}
