using Plugin.BLE.Abstractions.Contracts;
using Prism.Events;

namespace suota_pgp.Droid
{
    internal static class ManagerEvents
    {
        public class BluetoothStateChangedEvent : PubSubEvent<BluetoothState> { }
    }
}