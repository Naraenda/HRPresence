# HRPresence

Windows GATT heartrate monitor tool that pushes BPM to OpenSoundControl (OSC) and DiscordRPC.

```toml
# Restart if x seconds of no communication
time_out_interval = 4.0

# Wait x seconds before restarting in case of any errors
restart_delay = 4.0

# Enable Discord Rich Presence
use_discord_rpc = true

# The DiscordRPC Application ID
discord_rpcid = "385821357151223818"

# Enable OSC
use_osc = true

# the port to send OSC data to
oscport = 9000
```
