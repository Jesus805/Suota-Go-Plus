namespace suota_pgp
{
    public enum SpotarStatusUpdate
    {
        /// <summary>
        /// Valid memory device has been configured by initiator.
        /// </summary>
        ServiceStarted = 0x01,
        /// <summary>
        /// SPOTA process completed successfully. (Will never be called)
        /// </summary>
        CompletedOkay = 0x02,
        /// <summary>
        /// Forced exit of SPOTAR service.
        /// </summary>
        ServiceExit = 0x03,
        /// <summary>
        /// Overall Patch Data CRC failed.
        /// </summary>
        CrcError = 0x04,
        /// <summary>
        /// Received patch Length not equal to PATCH_LEN characteristic value.
        /// </summary>
        PatchLengthError = 0x05,
        /// <summary>
        /// External Mem Error (Writing to external device failed)
        /// </summary>
        ExtMemWriteError = 0x06,
        /// <summary>
        /// Internal Mem Error (not enough space for Patch)
        /// </summary>
        IntMemError = 0x07,
        /// <summary>
        /// Invalid memory device
        /// </summary>
        InvalidMemType = 0x08,
        /// <summary>
        /// Application error
        /// </summary>
        AppError = 0x09,
        /// <summary>
        /// SPOTA started for downloading image (SUOTA application).
        /// </summary>
        ImgStarted = 0x10,
        /// <summary>
        /// Invalid image bank.
        /// </summary>
        InvalidImgBank = 0x11,
        /// <summary>
        /// Invalid image header.
        /// </summary>
        InvalidImgHeader = 0x12,
        /// <summary>
        /// Invalid image size.
        /// </summary>
        InvalidImgSize = 0x13,
        /// <summary>
        /// Invalid product header.
        /// </summary>
        InvalidProductHeader = 0x14,
        /// <summary>
        /// Same Image Error.
        /// </summary>
        SameImgError = 0x15,
        /// <summary>
        /// Failed to read from external memory device.
        /// </summary>
        ExtMemReadError = 0x16,
        /// <summary>
        /// Firmware failed valid integrity check. 
        /// This will happen because our custom firmware is not signed. 
        /// However we will bypass it.
        /// </summary>
        GoPlusFailedIntegrity = 0x17
    }
}
