#include "rwip_config.h"             // SW configuration
#include "user_peripheral.h"
#include "user_custs1_impl.h"
#include "arch_api.h"
#include "user_custs1_def.h"
#include "gap.h"
#include "user_extractor.h"

/**
 ****************************************************************************************
 * @brief Enable OTP for reading.
 * @return void
 ****************************************************************************************
*/
inline void enable_read_otp()
{
    // Enable OTP clock
    SetBits16(CLK_AMBA_REG, OTP_ENABLE, 1);
    while ((GetWord16(ANA_STATUS_REG) & LDO_OTP_OK) != LDO_OTP_OK)
    // Set OTP in read mode
    SetWord32(OTPC_MODE_REG, OTPC_MODE_MREAD);
}

/**
 ****************************************************************************************
 * @brief Disable OTP.
 * @return void
 ****************************************************************************************
*/
inline void disable_read_otp()
{
    // Disable OTP clock
    SetBits16(CLK_AMBA_REG, OTP_ENABLE, 0);
}

/**
 ****************************************************************************************
 * @brief Extract Blob from OTP and write it to the Blob characteristic.
 * @return void
 ****************************************************************************************
*/
void init_blob()
{
    struct custs1_val_ntf_req* req = KE_MSG_ALLOC_DYN(CUSTS1_VAL_NTF_REQ,
                                                      TASK_CUSTS1,
                                                      TASK_APP,
                                                      custs1_val_ntf_req,
                                                      DEF_CUST1_BLOB_CHAR_LEN);
    uint8_t *blob = (uint8_t *)BLOB_ADDR;
    enable_read_otp();
    memcpy(req->value, blob, DEF_CUST1_BLOB_CHAR_LEN);
    disable_read_otp();

    req->conhdl = app_env->conhdl;
    req->handle = CUST1_IDX_BLOB_VAL;
    req->length = DEF_CUST1_BLOB_CHAR_LEN;

    ke_msg_send(req);
}

/**
 ****************************************************************************************
 * @brief Extract Key from OTP and write it to the Key characteristic.
 * @return void
 ****************************************************************************************
*/
void init_key()
{
    struct custs1_val_ntf_req* req = KE_MSG_ALLOC_DYN(CUSTS1_VAL_NTF_REQ,
                                                      TASK_CUSTS1,
                                                      TASK_APP,
                                                      custs1_val_ntf_req,
                                                      DEF_CUST1_KEY_CHAR_LEN);
    uint8_t *key = (uint8_t *)KEY_ADDR;
    enable_read_otp();
    memcpy(req->value, key, DEF_CUST1_KEY_CHAR_LEN);
    disable_read_otp();

    req->conhdl = app_env->conhdl;
    req->handle = CUST1_IDX_KEY_VAL;
    req->length = DEF_CUST1_KEY_CHAR_LEN;

    ke_msg_send(req);
}
