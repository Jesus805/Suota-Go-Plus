using System;
using System.Collections.Generic;

namespace suota_pgp.Model
{
    public static class Constants
    {
        public static readonly Guid ExtractorServiceUuid = Guid.Parse("edfec62e-9910-0bac-5241-d8bda6932a2f");

        public static readonly Guid KeyCharacteristicUuid = Guid.Parse("ce62c734-3592-a882-d849-f129a2ec6ce1");

        public static readonly Guid BlobCharacteristicUuid = Guid.Parse("b58b010d-6de0-7f92-3c47-4f36c70f3632");

        public static readonly Guid RestoreCharacteristicUuid = Guid.Parse("d216f679-4c8c-42f6-8206-a7150a0ec0fd");

        public static readonly Guid GoPlusServiceUuuid = Guid.Parse("21c50462-67cb-63a3-5c4c-82b5b9939aeb");

        public static readonly Guid GoPlusUpdateRequestUuid = Guid.Parse("21c50462-67cb-63a3-5c4c-82b5b9939aef");

        /// <summary>
        /// BLE Characteristic to BLE Service map.
        /// </summary>
        public static readonly Dictionary<Guid, Guid> Char2ServiceMap = new Dictionary<Guid, Guid>()
        {
            { KeyCharacteristicUuid, ExtractorServiceUuid },
            { BlobCharacteristicUuid, ExtractorServiceUuid },
            { RestoreCharacteristicUuid, ExtractorServiceUuid },
            { GoPlusUpdateRequestUuid, GoPlusServiceUuuid }
        };
    }
}
