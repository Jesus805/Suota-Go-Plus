using System;
using System.Collections.Generic;

namespace suota_pgp.Model
{
    public static class Constants
    {
        public static readonly byte[] EnableSuota = { 0x01 };

        public const int MemoryBank = 1;

        /// <summary>
        /// SPI Settings
        /// </summary>
        public const int SpiMemoryType      = 3;
        public const int SpiMiso            = 5;
        public const int SpiMosi            = 6;
        public const int SpiCs              = 3;
        public const int SpiSck             = 0;
        public const int BlockSize          = 240;
        public const int SpiMemTypeExternal = 0x13;

        /// <summary>
        /// SPOTA started for downloading image (SUOTA application).
        /// </summary>
        public const int SpotarImgStarted           = 0x10;
        /// <summary>
        /// Invalid image bank.
        /// </summary>
        public const int SpotarInvalidImgBank       = 0x11;
        /// <summary>
        /// Invalid image header.
        /// </summary>
        public const int SpotarInvalidImgHeader     = 0x12;
        /// <summary>
        /// Invalid image size.
        /// </summary>
        public const int SpotarInvalidImgSize       = 0x13;
        /// <summary>
        /// Invalid product header.
        /// </summary>
        public const int SpotarInvalidProductHeader = 0x14;
        /// <summary>
        /// Same Image Error.
        /// </summary>
        public const int SpotarSameImgError         = 0x15;
        /// <summary>
        /// Failed to read from external memory device.
        /// </summary>
        public const int SpotarExtMemReadError      = 0x16;

        /// <summary>
        /// Extractor Service Characteristics
        /// </summary>
        public static readonly Guid ExtractorServiceUuid      = Guid.Parse("edfec62e-9910-0bac-5241-d8bda6932a2f");
        /// <summary>
        /// Read Key - READ
        /// </summary>
        public static readonly Guid KeyCharacteristicUuid     = Guid.Parse("ce62c734-3592-a882-d849-f129a2ec6ce1");
        /// <summary>
        /// Read Blob - READ
        /// </summary>
        public static readonly Guid BlobCharacteristicUuid    = Guid.Parse("b58b010d-6de0-7f92-3c47-4f36c70f3632");
        /// <summary>
        /// Restore Go+ to it's original firmware - WRITE
        /// </summary>
        public static readonly Guid RestoreCharacteristicUuid = Guid.Parse("d216f679-4c8c-42f6-8206-a7150a0ec0fd");

        /// <summary>
        /// Go+ Service
        /// </summary>
        public static readonly Guid GoPlusServiceUuuid      = Guid.Parse("21c50462-67cb-63a3-5c4c-82b5b9939aeb");
        /// <summary>
        /// Update Request - READ, WRITE
        /// </summary>
        public static readonly Guid GoPlusUpdateRequestUuid = Guid.Parse("21c50462-67cb-63a3-5c4c-82b5b9939aef");

        /// <summary>
        /// SUOTA Service
        /// </summary>
        public static readonly Guid SpotaServiceUuid    = Guid.Parse("0000fef5-0000-1000-8000-00805f9b34fb");
        /// <summary>
        /// Patch Memory Device - READ, WRITE
        /// </summary>
        public static readonly Guid SpotaMemDevUuid     = Guid.Parse("8082caa8-41a6-4021-91c6-56f9b954cc34");
        /// <summary>
        /// GPIO Mapping - READ, WRITE
        /// </summary>
        public static readonly Guid SpotaGpioMapUuid    = Guid.Parse("724249f0-5eC3-4b5f-8804-42345af08651");
        /// <summary>
        /// Patch Memory Information - READ
        /// </summary>
        public static readonly Guid SpotaMemInfoUuid    = Guid.Parse("6c53db25-47a1-45fe-a022-7c92fb334fd4");
        /// <summary>
        /// Patch Length - READ, WRITE
        /// </summary>
        public static readonly Guid SpotaPatchLenUuid   = Guid.Parse("9d84b9a3-000c-49d8-9183-855b673fda31");
        /// <summary>
        /// Patch Data - READ, WRITE, WRITE NO RESPONSE
        /// </summary>
        public static readonly Guid SpotaPatchDataUuid  = Guid.Parse("457871e8-d516-4ca1-9116-57d0b17b9cb2");
        /// <summary>
        /// Patch Status - READ, NOTIFY
        /// </summary>
        public static readonly Guid SpotaServStatusUuid = Guid.Parse("5f78df94-798c-46f5-990a-b3eb6a065c88");

        /// <summary>
        /// BLE Characteristic to BLE Service map.
        /// </summary>
        public static readonly Dictionary<Guid, Guid> Char2ServiceMap = new Dictionary<Guid, Guid>()
        {
            { KeyCharacteristicUuid, ExtractorServiceUuid },
            { BlobCharacteristicUuid, ExtractorServiceUuid },
            { RestoreCharacteristicUuid, ExtractorServiceUuid },
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
