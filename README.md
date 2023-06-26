# Intelbras AMT Monitor

[EN]

This project started as a study of the communication protocol used by the Intelbras AMT-8000 alarm system, which lacks proper documentation.

The project is based on the captured packets using WireShark.

[PT-BR]

Este projeto começou para estudar a comunicação do alarme Intelbras AMT-8000 que tem uma documentação inexistente

O projeto é baseado nos pacotes capturados com WireShark

## Usage

This project was not intended for production use; however, the testing tools may be helpful in some cases.

The AMT.App project can be used interactively or in CLI mode.

## Available Commands

To use this application, you can use the following commands:

- `--help` or `/h`: Displays the help information for the available commands.
- `--cli`: Starts the command-line interface (CLI) mode.
- `--listen`: Activates the listen mode.
- `--ip`: Specifies the central IP address.
- `--port`: Specifies the central port number.
- `--pwd`: Specifies the central password.
- `--max-sensors` (CLI only): Sets the maximum number of sensors to display.

To execute any command, you can include it as an argument when running the application.

Example usage:

```
$ myApp.exe --cli
```

This command will start the application in CLI mode.

```
$ myApp.exe --ip 192.168.0.1 --port 8080 --pwd myPassword
```

This command will launch the application and connect to the central server using the specified IP address, port number, and password.

Please note that only the `--cli` command is available in the command-line interface mode, while the other commands can be used in both CLI and listen modes.

## Contributing

We welcome contributions to the Intelbras AMT Monitor project! However, it's important to note that none of Intelbras' intellectual property (IP), including their proprietary code, algorithms, or documentation, should be used or included in any contributions.

Please adhere to the following guidelines when making contributions:

1. **Respect Intellectual Property:** Do not use or include any Intelbras IP in your contributions. This includes code snippets, algorithms, documentation, or any other proprietary information owned by Intelbras.

2. **Original Work:** Ensure that your contributions are your original work and do not infringe upon any intellectual property rights of others.

3. **License Compatibility:** Make sure that your contributions are compatible with the project's open-source license, if applicable.

Please be aware that contributions that violate these guidelines, including those that contain any Intelbras IP, will not be accepted. If, by any chance, such contributions are mistakenly accepted, they will be promptly identified and removed from the project. We genuinely appreciate your understanding and cooperation in assisting us in maintaining a compliant and respectful project environment.

Additionally, we want to emphasize that individuals who have signed any non-disclosure agreements (NDAs) with Intelbras or have had access to Intelbras software development kits (SDKs) are not permitted to contribute to this project. We must respect legal obligations and protect intellectual property rights. If you fall into this category, we kindly request that you refrain from making contributions to the Intelbras AMT Monitor project. Thank you for your understanding and adherence to this policy.

If you have any questions or need further clarification, please reach out to the project maintainers. Thank you for your interest in contributing to the Intelbras AMT Monitor project!