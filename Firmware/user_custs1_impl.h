#ifndef _USER_CUSTS1_IMPL_H_
#define _USER_CUSTS1_IMPL_H_

#include "gapc_task.h"
#include "gapm_task.h"
#include "custs1_task.h"

/*
 * DEFINES
 ****************************************************************************************
 */

// Pokemon Go Plus Original firmware size
#define FIRMWARE_SIZE 32048
#define BLOCK_SIZE 256
#define BANK_1_ADDR 0x8000
#define BANK_2_ADDR 0x10000

#define SPI_GPIO_PORT  GPIO_PORT_0
#define SPI_CLK_PIN    GPIO_PIN_0
#define SPI_CS_PIN     GPIO_PIN_3
#define SPI_DI_PIN     GPIO_PIN_5
#define SPI_DO_PIN     GPIO_PIN_6

/*
 * FUNCTION DECLARATIONS
 ****************************************************************************************
 */

/**
 ****************************************************************************************
 * @brief Restore the original Pokemon Go Plus Firmware.
 * @param[in] msgid   Id of the message received.
 * @param[in] param   Pointer to the parameters of the message.
 * @param[in] dest_id ID of the receiving task instance.
 * @param[in] src_id  ID of the sending task instance.
 * @return void
 ****************************************************************************************
*/
void user_custs1_restore_wr_handler(ke_msg_id_t const msgid,
                                    struct custs1_val_write_ind const* param,
                                    ke_task_id_t const dest_id,
                                    ke_task_id_t const src_id);

/**
 ****************************************************************************************
 * @brief Set SPI GPIO Configuration
 * @return void
 ****************************************************************************************
*/
void app_spi_config(void);

/**
 ****************************************************************************************
 * @brief Initialize SPI
 * @return void
 ****************************************************************************************
*/
void app_spi_init(void);

/**
 ****************************************************************************************
 * @brief Erase spi blocks.
 * @param[in] address Starting address to erase.
 * @param[in] size    Size to erase in bytes.
 * @return void
 ****************************************************************************************
*/
int8_t app_spi_flash_erase(uint32_t address, uint32_t size);

/**
 ****************************************************************************************
 * @brief Set Restore Status (NOTIFY)
 * @return void
 ****************************************************************************************
*/
void restore_send_status_update_req(uint8_t status);

#endif // _USER_CUSTS1_IMPL_H_
