# Intelbras AMT Monitor

[![NuGet](https://buildstats.info/nuget/Simple.AMT)](https://www.nuget.org/packages/Simple.AMT)

[EN]

This project started as a study of the communication protocol used by the Intelbras AMT-8000 alarm system, which lacks proper documentation.

The project is based on the captured packets using WireShark.

[PT-BR]

Este projeto começou para estudar a comunicação do alarme Intelbras AMT-8000 que tem uma documentação inexistente

O projeto é baseado nos pacotes capturados com WireShark


- [Intelbras AMT Monitor](#intelbras-amt-monitor)
- [AMT.App Usage options](#amtapp-usage-options)
  - [Available Operation Modes](#available-operation-modes)
    - [Global Parameters](#global-parameters)
    - [Usage](#usage)
    - [CLI Mode](#cli-mode)
    - [JSON Mode](#json-mode)
  - [Using with Docker](#using-with-docker)
- [WebApp / API Usage options](#webapp--api-usage-options)
- [Contributing](#contributing)



This project was not intended for production use; however, the testing tools may be helpful in some cases.

The AMT.App project can be used interactively or in CLI mode.

# AMT.App Usage options

## Available Operation Modes

This application supports multiple interactions modes

To list all available options, use `--help` or `/h`

### Global Parameters

- `--help` or `/h`: Displays the help information for the available commands.
- `--ip`: Specifies the central IP address.
- `--port`: Specifies the central port number.
- `--pwd`: Specifies the central password.

### Usage

To execute any command, you can include it as an argument when running the application.

Example usage:

```
$ AMT.App.exe --cli
```

This command will start the application in CLI mode.

```
$ AMT.App.exe --ip 192.168.0.1 --port 8080 --pwd myPassword
```

This command will launch the application and connect to the central server using the specified IP address, port number, and password.

Please note that only the `--cli` command is available in the command-line interface mode, while the other commands can be used in both CLI and listen modes.

### CLI Mode

A very crude CLI based way of requesting information

Mostly used for debugging

Parameters:
- `--cli`: activates CLI mode
- `--max-sensors`: max sensors to display
- `--central-info`: outputs central information
- `--update`: continuously print changed sensors
- `--continuous`: continuously print sensors values (easy to parse)

### JSON Mode

Prints specific information as json

Parameters:
- `--json`: activates JSON mode
- `--central-info`: fetch central data
- `--central-mac`: fetch central mac"
- `--zone-names`: fetch zone names
- `--zone-types`: fetch zone types
- `--sensors-config`: fetch sensors configuration
- `--sensors`: fetch sensors states
- `--users`: fetch sensors states
- `--connections`: fetch connections states
- `--events`: fetch central events

## Using with Docker

Using this project inside a docker container

1. Save this Dockerfile to a directory:

```Dockerfile
# Dockerfile

# Base image with .NET SDK 6.0
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# Set the working directory
WORKDIR /app

# Clone this repository
RUN git clone https://github.com/RafaelEstevamReis/AMT-Monitor.git

# Change directory to the cloned repository
WORKDIR /app/AMT-Monitor

# Build the project
RUN dotnet build -c Release

# Run the project
CMD ["dotnet", "run", "--project", "AMT.App/AMT.App.csproj"]
```

2. Build the Docker image using the following command:
```
docker build -t amt-monitor .
```
3. After the image is successfully built, you can run a container based on the image:
```
docker run -it amt-monitor [execution arguments]
```

Replace `[execution arguments]` with the desired arguments such as `--cli`, `--ip`, or any other applicable arguments based on the project's command-line options.

Note that:
1. You have to provide all needed parameters:
* IP
* Password

And may need to provide
* Port

2. This must run from the same network as the AMT, cloud access is not supported

# WebApp / API Usage options



# Contributing

We welcome contributions to the project! However, it's important to note that none of Intelbras' intellectual property (IP), including their proprietary code, algorithms, or documentation, should be used or included in any contributions.

Please adhere to the following guidelines when making contributions:

1. **Respect Intellectual Property:** Do not use or include any Intelbras IP in your contributions. This includes code snippets, algorithms, documentation, or any other proprietary information owned by Intelbras.

2. **Original Work:** Ensure that your contributions are your original work and do not infringe upon any intellectual property rights of others.

3. **License Compatibility:** Make sure that your contributions are compatible with the project's open-source license, if applicable.

Please be aware that contributions that violate these guidelines, including those that contain any Intelbras IP, will not be accepted. If, by any chance, such contributions are mistakenly accepted, they will be promptly identified and removed from the project. We genuinely appreciate your understanding and cooperation in assisting us in maintaining a compliant and respectful project environment.

Additionally, we want to emphasize that individuals who have signed any non-disclosure agreements (NDAs) with Intelbras or have had access to Intelbras software development kits (SDKs) are not permitted to contribute to this project's library. We must respect legal obligations and protect intellectual property rights. If you fall into this category, we kindly request that you refrain from making this contributions. Thank you for your understanding and adherence to this policy.

If you have any questions or need further clarification, please reach out to the project maintainers. Thank you for your interest in contributing to the project!
