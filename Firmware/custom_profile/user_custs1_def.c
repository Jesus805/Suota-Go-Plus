#include <stdint.h>
#include "prf_types.h"
#include "attm_db_128.h"
#include "user_custs1_def.h"

static att_svc_desc128_t custs1_svc                         = DEF_CUST1_SVC_UUID_128;

static uint8_t CUST1_KEY_UUID_128[ATT_UUID_128_LEN]         = DEF_CUST1_KEY_UUID_128;

static uint8_t CUST1_BLOB_UUID_128[ATT_UUID_128_LEN]        = DEF_CUST1_BLOB_UUID_128;

static const struct att_char128_desc custs1_key_char        = {ATT_CHAR_PROP_RD,
													          {0, 0},
												  	          DEF_CUST1_KEY_UUID_128};

static const struct att_char128_desc custs1_blob_char       = {ATT_CHAR_PROP_RD,
													          {0, 0},
                                                              DEF_CUST1_BLOB_UUID_128};


static uint16_t att_decl_svc       = ATT_DECL_PRIMARY_SERVICE;
static uint16_t att_decl_char      = ATT_DECL_CHARACTERISTIC;
static uint16_t att_decl_user_desc = ATT_DESC_CHAR_USER_DESCRIPTION;

/// Full CUSTOM1 Database Description - Used to add attributes into the database
struct attm_desc_128 custs1_att_db[CUST1_IDX_NB] =
{
    // CUSTOM1 Service Declaration
    [CUST1_IDX_SVC]                = {(uint8_t*)&att_decl_svc, ATT_UUID_16_LEN, PERM(RD, ENABLE),
                                      sizeof(custs1_svc), sizeof(custs1_svc), (uint8_t*)&custs1_svc},

    // Key Extract Characteristic Declaration
    [CUST1_IDX_KEY_CHAR]           = {(uint8_t*)&att_decl_char, ATT_UUID_16_LEN, PERM(RD, ENABLE),
		                               sizeof(custs1_key_char), sizeof(custs1_key_char), (uint8_t*)&custs1_key_char},

    // Key Extract Characteristic Value
    [CUST1_IDX_KEY_VAL]            = {CUST1_KEY_UUID_128, ATT_UUID_128_LEN, PERM(RD, ENABLE),
		                              DEF_CUST1_KEY_CHAR_LEN, DEF_CUST1_KEY_CHAR_LEN, (uint8_t*)0x40000},

	// Key Extract Characteristic User Description
	[CUST1_IDX_KEY_USER_DESC]      = {(uint8_t*)&att_decl_user_desc, ATT_UUID_16_LEN, PERM(RD, ENABLE),
		                              sizeof(CUST1_KEY_USER_DESC) - 1, sizeof(CUST1_KEY_USER_DESC) - 1, (uint8_t*)CUST1_KEY_USER_DESC},

	// Blob Extract Characteristic Declaration
	[CUST1_IDX_BLOB_CHAR]          = {(uint8_t*)&att_decl_char, ATT_UUID_16_LEN, PERM(RD, ENABLE),
		                              sizeof(custs1_blob_char), sizeof(custs1_blob_char), (uint8_t*)&custs1_blob_char},

	// Blob Extract Characteristic Value
	[CUST1_IDX_BLOB_VAL]           = {CUST1_BLOB_UUID_128, ATT_UUID_128_LEN, PERM(RD, ENABLE),
		                              DEF_CUST1_BLOB_CHAR_LEN, DEF_CUST1_BLOB_CHAR_LEN, (uint8_t*)0x40100},

	// Blob Extract Characteristic User Description
    [CUST1_IDX_BLOB_USER_DESC]     = {(uint8_t*)&att_decl_user_desc, ATT_UUID_16_LEN, PERM(RD, ENABLE),
		                              sizeof(CUST1_BLOB_USER_DESC) - 1, sizeof(CUST1_BLOB_USER_DESC) - 1, (uint8_t*)CUST1_BLOB_USER_DESC}
};

/// @} USER_CONFIG
