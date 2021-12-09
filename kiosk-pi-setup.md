# setup Pi Zero W (2)

## basic

### on some other machine

1. download + write to (micro)SD **Raspberry Pi OS Lite (32-bit)** with **Raspberry Pi Imager**
1. re-insert (micro)SD
1. on the SD:
   - create an empty `ssh` file
   - create a `wpa_supplicant.conf` with a working configuration like

```text
ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev
update_config=1
country={country-code}

network={
        ssid="{ssid}"
        psk="{ssid-password}"
}
```

### with / on Pi

1. insert microSD and power on
1. `ssh pi@raspberrypi`
   - login with initial password `raspberry`
1. `passwd` to set new `pi` password
1. rename host + change timezone + change keyboard layout

```shell
echo familyboard | sudo tee /etc/hostname
sudo unlink /etc/localtime && sudo ln -s /usr/share/zoneinfo/Europe/Berlin /etc/localtime
sudo sed -i -e 's/XKBLAYOUT="[a-z][a-z]"/XKBLAYOUT="de"/' /etc/default/keyboard
```

5. turn off Bluetooth

```shell
sudo apt-get purge bluez -y
sudo apt-get clean
sudo apt-get autoremove -y
```
1. `sudo apt update && sudo apt upgrade -y`

7. disable IPv6 `sudo nano /etc/sysctl.conf` - set `net.ipv6.conf.all.disable_ipv6=1`
1. reboot `sudo reboot now`
1. from here on `ssh pi@familyboard` with **password** set above

----

## SSH

## new key

- setup SSH on Pi

```
ssh-keygen
cat ~/.ssh/id_rsa.pub >> ~/.ssh/authorized_keys
```

- setup SSH on remote

```
scp pi@familyboard:/home/pi/.ssh/id_rsa ~/.ssh/familyboard
```
- add to remote `.ssh/config`

```
host familyboard
  HostName familyboard
  User pi
  IdentityFile ~/.ssh/familyboard
```

## existing public key on remote

- setup SSH from remote

```shell
ssh pi@familyboard mkdir -p /home/pi/.ssh
scp ~\.ssh\familyboard.pub pi@familyboard:/home/pi/.ssh/authorized_keys
```

----

## install desktop

- disable overscan <http://martinpennock.com/blog/how-to-remove-black-border-from-raspberry-pi-display/>

- install chromium + window manager

```shell
sudo apt-get install -y chromium-browser
sudo apt-get install -y xserver-xorg xserver-xorg-video-fbturbo xinit
sudo apt-get install -y matchbox-window-manager unclutter xdotool
```

- add to `.profile`

```
[[ -z $DISPLAY && $XDG_VTNR -eq 1 ]] && exec xinit ./startboard.sh
```

- create autologin folder and set autologin to text console

```
sudo mkdir -pv /etc/systemd/system/getty@tty1.service.d
sudo raspi-config
```

- setup `crontab -e` to turn off display at 22h01

```
1 22 * * * export DISPLAY=:0.0 && vcgencmd display_power 0
```

- setup `sudo crontab -e` to shutdown at 22h02

```
2 22 * * /sbin/shutdown -h 0
```

- on `/home/pi` create `startboard.sh`

```shell
#!/bin/sh
xset s noblank
xset s off
xset -dpms

unclutter -idle 0.5 -root &

export DISPLAY=:0.0
export XAUTHORITY=/home/pi/.Xauthority

matchbox-window-manager -use_cursor no -use_titlebar no &

chromium-browser --noerrdialogs --check-for-update-interval=1 --simulate-critical-update --incognito --disable-infobars --kiosk --start-fullscreen http://homeserver.fritz.box:5000/
```

- make it executable

```
chmod +x /home/pi/*.sh
```

- reboot