#ifndef _USER_EXTRACTOR_DEF_H_
#define _USER_EXTRACTOR_DEF_H_

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
