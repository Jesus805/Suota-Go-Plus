#ifndef _USER_CUSTS1_DEF_H_
#define _USER_CUSTS1_DEF_H_

#include "attm_db_128.h"

// Custom Service #1 UUID
#define DEF_CUST1_SVC_UUID_128      {0xDC, 0x6B, 0xB1, 0x9B, 0xAF, 0x07, 0x48, 0xAA, 0x95, 0x58, 0x3B, 0x1F, 0x76, 0x8F, 0x5D, 0x84}
// Key Characteristic UUID
#define DEF_CUST1_KEY_UUID_128      {0x58, 0xEA, 0x33, 0x5C, 0x7F, 0xA9, 0x46, 0x57, 0x8A, 0xB8, 0xBD, 0x20, 0xB1, 0x5A, 0x0D, 0x87}
// Blob Characteristic UUID
#define DEF_CUST1_BLOB_UUID_128     {0x18, 0x1D, 0x38, 0xDF, 0x0A, 0xB4, 0x41, 0xB1, 0xB2, 0xF1, 0xE3, 0xF8, 0xAF, 0x02, 0x00, 0xFE}

// Key Characteristic Data Length (Key is 16 bytes)
#define DEF_CUST1_KEY_CHAR_LEN        16
// Blob Characteristic Data Length (BLOB is 256 bytes)
#define DEF_CUST1_BLOB_CHAR_LEN       256
// Restore Characteristic Data Length
#define DEF_CUST1_RESTORE_CHAR_LEN    1

// Key Characteristic Description Name
#define CUST1_KEY_USER_DESC               "Go+ Encryption Key"
// Blob Characteristic Description Name
#define CUST1_BLOB_USER_DESC              "Go+ Plus Blob"
// Restore Characteristic Description Name
#define CUST1_RESTORE_USER_DESC           "Restore Original Firmware"

/// Custom1 Service Data Base Characteristic enum
enum
{
    CUST1_IDX_SVC = 0,

		CUST1_IDX_KEY_CHAR,
		CUST1_IDX_KEY_VAL,
		CUST1_IDX_KEY_USER_DESC,

		CUST1_IDX_BLOB_CHAR,
		CUST1_IDX_BLOB_VAL,
		CUST1_IDX_BLOB_USER_DESC,

    CUST1_IDX_NB
};

/*
 * GLOBAL VARIABLE DECLARATIONS
 ****************************************************************************************
 */

// Defined in user_custs1_def.c
extern struct attm_desc_128 custs1_att_db[CUST1_IDX_NB];

/// @} USER_CONFIG

#endif // _USER_CUSTS1_DEF_H_
