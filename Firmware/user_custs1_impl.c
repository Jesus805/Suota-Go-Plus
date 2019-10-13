#include "gpio.h"
#include "app_api.h"
#include "app.h"
#include "user_custs1_def.h"
#include "user_custs1_impl.h"
#include "user_peripheral.h"
#include "user_periph_setup.h"
#include "spi_flash.h"
#include "spi.h"

static uint8_t block[BLOCK_SIZE];
uint8_t pgp_restored = 0;

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
	ke_task_id_t const src_id)
{
	uint8_t val = 0;
	memcpy(&val, &param->value[0], param->length);

	if (val == 0x01) {

		app_spi_config();
		app_spi_init();

		uint32_t n = FIRMWARE_SIZE / BLOCK_SIZE + ((FIRMWARE_SIZE % BLOCK_SIZE == 0) ? 0 : 1);
		uint32_t block_size = BLOCK_SIZE;

		uint32_t bank_1_addr = BANK_1_ADDR;
		uint32_t bank_2_addr = BANK_2_ADDR;

		spi_cs_low();
		spi_cs_high();

		// Bank 1 must be wiped before writing
		app_spi_flash_erase(BANK_1_ADDR, FIRMWARE_SIZE);

		for (uint32_t i = 0; i < n; i++) {
			// Final Block
			if (i == n - 1) {
				if (FIRMWARE_SIZE % BLOCK_SIZE != 0) {
					block_size = FIRMWARE_SIZE % BLOCK_SIZE;
				}
			}

			int32_t result = spi_flash_read_data(block, bank_2_addr, block_size);
			if (result <= 0) {
				restore_send_status_update_req((uint8_t)result);
				return;
			}

			// Set the image ID to 1
			if (i == 0) {
				block[3] = 0x01;
			}

			result = spi_flash_write_data(block, bank_1_addr, block_size);
			if (result < 0) {
				restore_send_status_update_req((uint8_t)result);
				return;
			}

			bank_1_addr += block_size;
			bank_2_addr += block_size;
		}

		restore_send_status_update_req(1);

		spi_release();
		pgp_restored = 1;
	}
}

/**
 ****************************************************************************************
 * @brief Set SPI GPIO Configuration
 * @return void
 ****************************************************************************************
*/
void app_spi_config()
{
	SetWord16(CLK_AMBA_REG, 0x00);
	SetWord16(SET_FREEZE_REG, FRZ_WDOG);
	SetBits16(SYS_CTRL_REG, PAD_LATCH_EN, 1);
	SetBits16(PMU_CTRL_REG, PERIPH_SLEEP, 0);

	SetBits16(PMU_CTRL_REG, PERIPH_SLEEP, 0);
	while (!(GetWord16(SYS_STAT_REG) & PER_IS_UP));

	GPIO_ConfigurePin(SPI_GPIO_PORT, SPI_CS_PIN, OUTPUT, PID_SPI_EN, true);
	GPIO_ConfigurePin(SPI_GPIO_PORT, SPI_CLK_PIN, OUTPUT, PID_SPI_CLK, false);
	GPIO_ConfigurePin(SPI_GPIO_PORT, SPI_DO_PIN, OUTPUT, PID_SPI_DO, false);
	GPIO_ConfigurePin(SPI_GPIO_PORT, SPI_DI_PIN, INPUT, PID_SPI_DI, false);
}

/**
 ****************************************************************************************
 * @brief Initialize SPI
 * @return void
 ****************************************************************************************
*/
void app_spi_init()
{
	SPI_Pad_t cs_pad;
	cs_pad.pin = SPI_CS_PIN;
	cs_pad.port = SPI_GPIO_PORT;

	uint32_t flash_size = SPI_FLASH_DEFAULT_SIZE;
	uint32_t flash_page = SPI_FLASH_DEFAULT_PAGE;

	// Enable SPI & SPI FLASH
	spi_init(&cs_pad, SPI_MODE_8BIT, SPI_ROLE_MASTER, SPI_CLK_IDLE_POL_LOW, SPI_PHA_MODE_0, SPI_MINT_DISABLE, SPI_XTAL_DIV_8);

	uint16_t index = spi_read_flash_memory_man_and_dev_id();

	spi_flash_init(flash_size, flash_page);
}

/**
 ****************************************************************************************
 * @brief Erase spi blocks.
 * @param[in] address Starting address to erase.
 * @param[in] size    Size to erase in bytes.
 * @return void
 ****************************************************************************************
*/
int8_t app_spi_flash_erase(uint32_t address, uint32_t size)
{
	int8_t result = -1;

	uint32_t sector_addr = (address / SPI_SECTOR_SIZE) * SPI_SECTOR_SIZE;

	uint32_t sector_count = (size / SPI_SECTOR_SIZE) + ((size % SPI_SECTOR_SIZE) ? 1 : 0);

	for (uint32_t i = 0; i < sector_count; i++)
	{
		result = spi_flash_block_erase(sector_addr, SECTOR_ERASE);

		if (result != ERR_OK)
			return result;

		sector_addr += SPI_SECTOR_SIZE;
	}

	return result;
}

/**
 ****************************************************************************************
 * @brief Set Restore Status (NOTIFY)
 * @return void
 ****************************************************************************************
*/
void restore_send_status_update_req(uint8_t status)
{
	struct custs1_val_ntf_req* req = KE_MSG_ALLOC_DYN(CUSTS1_VAL_NTF_REQ,
		TASK_CUSTS1,
		TASK_APP,
		custs1_val_ntf_req,
		DEF_CUST1_RESTORE_STATUS_CHAR_LEN);

	req->value[0] = status;
	req->conhdl = app_env->conhdl;
	req->handle = CUST1_IDX_RESTORE_STATUS_VAL;
	req->length = DEF_CUST1_RESTORE_STATUS_CHAR_LEN;

	ke_msg_send(req);
}
