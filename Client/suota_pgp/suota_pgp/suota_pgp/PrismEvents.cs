using Prism.Events;
using suota_pgp.Model;

namespace suota_pgp
{
    public static class PrismEvents
    {
        public class AppStateChangedEvent : PubSubEvent<AppState> { }

        public class BluetoothStateChangedEvent : PubSubEvent { }

        public class CharacteristicUpdatedEvent : PubSubEvent<CharacteristicUpdate> { }

        public class ErrorStateChangedEvent : PubSubEvent<ErrorState> { }

        public class GoPlusFoundEvent : PubSubEvent<GoPlus> { }

        public class PermissionStateChangedEvent : PubSubEvent<PermissionState> { }

        public class ProgressUpdateEvent : PubSubEvent<Progress> { }

        public class RestoreCompleteEvent : PubSubEvent { }
    }
}