using Prism.Events;
using suota_pgp.Model;

namespace suota_pgp
{
    public static class PrismEvents
    {
        public class ScanStateChangeEvent : PubSubEvent<ScanState> { }

        public class GoPlusFoundEvent : PubSubEvent<GoPlus> { }

        public class FileLoadedEvent : PubSubEvent { }

        public class CharacteristicUpdatedEvent : PubSubEvent<CharValue> { }
    }
}
