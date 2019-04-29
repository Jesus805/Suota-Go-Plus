# DA14580 Custom Server Firmware

The code in this directory can be used in conjunction with the DA14580 SDK. Simply replace the SDK's *user_custs1_def.c* and *user_custs1_def.h* with the ones in this directory.
You may choose any *ble_app_{}*; I chose to use *ble_app_peripheral* since I didn't need OTA code.

The functions in *user_extractor.h* should be called at the end of *user_app_connection()* in *user_peripheral.c* like so

```
void user_app_connection(uint8_t connection_idx, struct gapc_connection_req_ind const *param)
 
{
    ...
    init_blob();

    init_key();
}
```

You may set your own device name in *user.config.h*