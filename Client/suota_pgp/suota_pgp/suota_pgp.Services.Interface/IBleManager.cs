using suota_pgp.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace suota_pgp.Services.Interface
{
    public interface IBleManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Found GO+ Devices during scan.
        /// </summary>
        ObservableCollection<GoPlus> BondedDevices { get; }

        GoPlus SelectedBondedDevice { get; set; }

        /// <summary>
        /// Found GO+ devices that are patched.
        /// </summary>
        ObservableCollection<GoPlus> ScannedDevices { get; }

        GoPlus SelectedScannedDevice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceGuid"></param>
        /// <returns></returns>
        Task<GoPlus> ConnectToKnownDevice(Guid deviceGuid);
        void Clear();
        void GetBondedDevices(string name, Guid service);
        void RemoveBond(GoPlus device);
        void Scan(Guid serviceUuid);
        void StopScan();
    }
}