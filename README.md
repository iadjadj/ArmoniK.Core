[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-green.svg)](https://www.gnu.org/licenses/agpl-3.0)

# ArmoniK.Core

| Stable                                                                                                                                                                                                                                                       | Edge                                                                                                                                                                                                                                                       |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_pollingagent?color=fe5001&label=armonik_pollingagent&sort=semver)](https://hub.docker.com/r/dockerhubaneo/armonik_pollingagent)                                        | [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_pollingagent?color=fe5001&label=armonik_pollingagent&sort=date)](https://hub.docker.com/r/dockerhubaneo/armonik_pollingagent)                                        |
| [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_control_metrics?color=fe5001&label=armonik_control_metrics&sort=semver)](https://hub.docker.com/r/dockerhubaneo/armonik_control_metrics)                               | [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_control_metrics?color=fe5001&label=armonik_control_metrics&sort=date)](https://hub.docker.com/r/dockerhubaneo/armonik_control_metrics)                               |
| [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_control_partition_metrics?color=fe5001&label=armonik_control_partition_metrics&sort=semver)](https://hub.docker.com/r/dockerhubaneo/armonik_control_partition_metrics) | [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_control_partition_metrics?color=fe5001&label=armonik_control_partition_metrics&sort=date)](https://hub.docker.com/r/dockerhubaneo/armonik_control_partition_metrics) |
| [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_control?color=fe5001&label=armonik_control&sort=semver)](https://hub.docker.com/r/dockerhubaneo/armonik_control)                                                       | [![Docker image latest version](https://img.shields.io/docker/v/dockerhubaneo/armonik_control?color=fe5001&label=armonik_control&sort=date)](https://hub.docker.com/r/dockerhubaneo/armonik_control)                                                       |


 ## What is ArmoniK.Core?

This project is part of the [ArmoniK](https://github.com/aneoconsulting/ArmoniK) project. ArmoniK.Core is responsible for the implementation of the services needed for ArmoniK which are defined in [ArmoniK.Api](https://github.com/aneoconsulting/ArmoniK.Api).

ArmoniK.Core provides services for submitting computational tasks, keeping track of the status of the tasks and retrieving the results of the computations. The tasks are processed by external workers whose interfaces are also defined in ArmoniK.Api. ArmoniK.Core sends tasks to the workers, manages eventual errors during the execution of the tasks and manages also the storage of the task's results.

More detailed information on the inner working of ArmoniK.Core is available [here](https://aneoconsulting.github.io/ArmoniK.Core/).

## Installation

ArmoniK.Core can be installed only on Linux machines. For Windows users, it is possible to do it on [WSL2](https://learn.microsoft.com/en-us/windows/wsl/about).

### Prerequisites

- [Terraform](https://www.terraform.io/) version >= 1.4.2
- [Just](https://github.com/casey/just) >= 1.8.0
- [Dotnet](https://dotnet.microsoft.com/en-us/) >= 6.0
- [Docker](https://www.docker.com/) >= 20.10.16
- [GitHub CLI](https://cli.github.com/) >= 2.23.0 (optional)

### Local deployment

To deploy ArmoniK.Core locally, you need first to clone the repository [ArmoniK.Core](https://github.com/aneoconsulting/armonik.core). Then, to see all the available recipes for deployment, place yourself at the root of the repository ArmoniK.Core where the justfile is located, and type on your command line:

```shell
just
```

More about local deployment, see [Local Deployment of ArmoniK.Core](./.docs/content/1.concepts/1.local-deployment.md).

### Tests

There are a number of tests that help to verify the successful installation of ArmoniK.Core. Some of them require a full deployment of ArmoniK.Core, for others a partial deployment is enough.

More about tests, see [Tests of ArmoniK.Core](./.docs/content//1.concepts/2.tests.md).

## Contribution

Contributions are always welcome!

See [ArmoniK.Community](https://github.com/aneoconsulting/ArmoniK.Community) for ways to get started.
