

// From user_custs1_impl.c
extern uint8_t pgp_restored;

void user_app_disconnect(struct gapc_disconnect_ind const *param)
{
    /* Cancel the parameter update request timer code here */

    if (pgp_restored)
    {
        // PGP Original firmware restored, reboot the device.
        SetBits16(SYS_CTRL_REG, SW_RESET, 1);
    }
    else
    {
        /* Restart advertising code here */
    }
}

void user_catch_rest_hndl(ke_msg_id_t const msgid,
                        void const *param,
                        ke_task_id_t const dest_id,
                        ke_task_id_t const src_id)
{
    switch(msgid)
    {
        case CUSTS1_VAL_WRITE_IND:
        {
            struct custs1_val_write_ind const *msg_param = (struct custs1_val_write_ind const *)(param);

            switch (msg_param->handle)
            {
                case CUST1_IDX_RESTORE_VAL:
                {
                    user_custs1_restore_wr_handler(msgid, msg_param, dest_id, src_id);
                } break;
            }
        } break;
        
        /* Rest of function here */
    }
}