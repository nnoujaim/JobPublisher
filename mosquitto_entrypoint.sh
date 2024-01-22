#!/bin/sh

# Create the mosquitto directory if it doesn't exist
mkdir -p /etc/mosquitto

# Create the mosquitto_passwd file using environment variables
if [ -z "$MOSQUITTO_USER" ] || [ -z "$MOSQUITTO_PASSWORD" ]; then
    echo "Environment variables MOSQUITTO_USER and MOSQUITTO_PASSWORD are required."
    exit 1
fi

echo "$MOSQUITTO_USER:$MOSQUITTO_PASSWORD" > /etc/mosquitto/passwd
mosquitto_passwd -U /etc/mosquitto/passwd

# Correct file permissions
chmod 0700 /etc/mosquitto/passwd
chown mosquitto:mosquitto /etc/mosquitto/passwd

# Start Mosquitto with the specified configuration file
exec /usr/sbin/mosquitto -c /mosquitto/config/mosquitto.conf
