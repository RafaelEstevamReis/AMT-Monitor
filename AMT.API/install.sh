#!/bin/bash

# Download Dockerfile
curl -O https://raw.githubusercontent.com/RafaelEstevamReis/AMT-Monitor/main/AMT.API/Dockerfile

# Remove existing container if it exists
if docker container inspect amt-api >/dev/null 2>&1; then
    docker stop amt-api >/dev/null && docker rm amt-api >/dev/null
fi
# Remove existing image if it exists
if docker image inspect amt-api-image >/dev/null 2>&1; then
    docker image rm amt-api-image >/dev/null
fi

# Build Docker image
docker build --no-cache -t amt-api-image .

# Prompt for IP input
read -p "Enter Central IP Address: " ip

# Prompt for PWD input (password masking)
read -s -p "Enter Central Password: " pwd
echo ""

# Run Docker container with IP and PWD as environment variables
docker run -d --restart unless-stopped --name amt-api -p 80:80 -e ip="$ip" -e pwd="$pwd" amt-api-image

echo "Installation complete"

