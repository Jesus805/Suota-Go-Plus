﻿using System;
using System.Collections.Generic;

namespace suota_pgp.Model
{
    public static class Constants
    {
        public const byte EnableSuota = 1;

        public const int MemoryBank = 1;

        public const int PatchAddress = 0x8002;

        public const int PatchLength = 6;

        public const int RetryCount = 3;

        public const int HeaderSize = 0x40;

        public const int DelayMS = 10;

        /// <summary>
        /// Suota Commands
        /// </summary>
        public const int SpotaImgEnd         = 0xFE;
        public const int SpotaMemServiceExit = 0xFF;

        /// <summary>
        /// SPI Settings
        /// </summary>
        public const int SpiMemoryType = 0x13;
        public const int SpiMiso       = 5;
        public const int SpiMosi       = 6;
        public const int SpiCs         = 3;
        public const int SpiSck        = 0;
        public const int BlockSize     = 240;
        public const int ChunkSize     = 20;

        public const string GoPlusName = "Pokemon GO Plus";

        /// <summary>
        /// Extractor Service Characteristics
        /// </summary>
        public static readonly Guid ExtractorServiceUuid      = Guid.Parse("845d8f76-1f3b-5895-aa48-07af9bb16bdc");
        /// <summary>
        /// Read Device Key - READ
        /// </summary>
        public static readonly Guid DeviceKeyCharacteristicUuid     = Guid.Parse("870d5ab1-20bd-b88a-5746-a97f5c33ea58");
        /// <summary>
        /// Read Blob Key - READ
        /// </summary>
        public static readonly Guid BlobKeyCharacteristicUuid    = Guid.Parse("fe0002af-f8e3-f1b2-b141-b40adf381d18");
        /// <summary>
        /// Restore Go+ to it's original firmware - WRITE
        /// </summary>
        public static readonly Guid RestoreCharacteristicUuid = Guid.Parse("6b64be6f-5467-d8b5-7143-1716be1b96be");
        /// <summary>
        /// Restore State Status - READ, NOTIFY
        /// </summary>
        public static readonly Guid RestoreCharacteristicStatusUuid = Guid.Parse("a258f13e-9559-a498-b144-d863a013db06");
        /// <summary>
        /// Dialog Semiconductor's proprietary SUOTA Service.
        /// </summary>
        public static readonly byte[] SuotaAdvertisementUuid = { 0xFE, 0xF5 };
        /// <summary>
        /// Go+ Service
        /// </summary>
        public static readonly Guid GoPlusServiceUuuid = Guid.Parse("21c50462-67cb-63a3-5c4c-82b5b9939aeb");
        /// <summary>
        /// Update Request - READ, WRITE
        /// </summary>
        public static readonly Guid GoPlusUpdateRequestUuid = Guid.Parse("21c50462-67cb-63a3-5c4c-82b5b9939aef");
        /// <summary>
        /// SUOTA Service
        /// </summary>
        public static readonly Guid SpotaServiceUuid = Guid.Parse("0000fef5-0000-1000-8000-00805f9b34fb");
        /// <summary>
        /// Patch Memory Device - READ, WRITE
        /// </summary>
        public static readonly Guid SpotaMemDevUuid = Guid.Parse("8082caa8-41a6-4021-91c6-56f9b954cc34");
        /// <summary>
        /// GPIO Mapping - READ, WRITE
        /// </summary>
        public static readonly Guid SpotaGpioMapUuid = Guid.Parse("724249f0-5eC3-4b5f-8804-42345af08651");
        /// <summary>
        /// Patch Memory Information - READ
        /// </summary>
        public static readonly Guid SpotaMemInfoUuid = Guid.Parse("6c53db25-47a1-45fe-a022-7c92fb334fd4");
        /// <summary>
        /// Patch Length - READ, WRITE
        /// </summary>
        public static readonly Guid SpotaPatchLenUuid = Guid.Parse("9d84b9a3-000c-49d8-9183-855b673fda31");
        /// <summary>
        /// Patch Data - READ, WRITE, WRITE NO RESPONSE
        /// </summary>
        public static readonly Guid SpotaPatchDataUuid = Guid.Parse("457871e8-d516-4ca1-9116-57d0b17b9cb2");
        /// <summary>
        /// Patch Status - READ, NOTIFY
        /// </summary>
        public static readonly Guid SpotaServStatusUuid = Guid.Parse("5f78df94-798c-46f5-990a-b3eb6a065c88");

        /// <summary>
        /// BLE Characteristic to BLE Service map.
        /// </summary>
        public static readonly Dictionary<Guid, Guid> Char2ServiceMap = new Dictionary<Guid, Guid>()
        {
            { DeviceKeyCharacteristicUuid, ExtractorServiceUuid },
            { BlobKeyCharacteristicUuid, ExtractorServiceUuid },
            { RestoreCharacteristicUuid, ExtractorServiceUuid },
            { RestoreCharacteristicStatusUuid, ExtractorServiceUuid },

            { GoPlusUpdateRequestUuid, GoPlusServiceUuuid },

            { SpotaMemDevUuid, SpotaServiceUuid },
            { SpotaGpioMapUuid, SpotaServiceUuid },
            { SpotaMemInfoUuid, SpotaServiceUuid },
            { SpotaPatchLenUuid, SpotaServiceUuid },
            { SpotaPatchDataUuid, SpotaServiceUuid },
            { SpotaServStatusUuid, SpotaServiceUuid }
        };
    }
}