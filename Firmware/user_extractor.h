#ifndef _USER_EXTRACTOR_DEF_H_
#define _USER_EXTRACTOR_DEF_H_

/*
 * DEFINES
 ****************************************************************************************
 */

#define OTPC_MODE_STDBY 0x00
#define OTPC_MODE_MREAD 0x01

#define BLOB_ADDR 0x47000
#define KEY_ADDR 0x47120

/*
 * FUNCTION DECLARATIONS
 ****************************************************************************************
 */

/**
 ****************************************************************************************
 * @brief Enable OTP for reading.
 * @return void
 ****************************************************************************************
*/
void enable_read_otp(void);

/**
 ****************************************************************************************
 * @brief Disable OTP.
 * @return void
 ****************************************************************************************
*/
void disable_read_otp(void);

/**
 ****************************************************************************************
 * @brief Extract Blob from OTP and write it to the Blob characteristic.
 * @return void
 ****************************************************************************************
*/
void init_blob(void);

/**
 ****************************************************************************************
 * @brief Extract Key from OTP and write it to the Key characteristic.
 * @return void
 ****************************************************************************************
*/
void init_key(void);

/// @} USER_CONFIG

#endif // _USER_EXTRACTOR_DEF_H_
