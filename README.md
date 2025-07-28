# About the Project

Frierun is an open-source project that aims to simplify the deployment and management of self-hosted applications.
It allows easy one-click installation and uninstallation of packages, yet it is highly configurable.

It introduces an abstract package format for defining applications and their dependencies. 
These abstract format definitions can be written once and reused everywhere by everyone: 
packages can be deployed on different clouds and infrastructures without any changes.

## Current state of the project

This project is at the early stages of development. It is not recommended to use it in production. 
It is more like a proof of concept right now.

# Installation

The easiest way to run Frierun is to use Docker. You might also use Podman instead if you prefer.

## Docker

Install Docker on your machine. You can find instructions [here](https://docs.docker.com/get-docker/).

Open the terminal and run Frierun container:

```bash
docker run -d \
  --name frierun \
  -p 8080:8080 \
  -v frierun-config:/App/Frierun \
  -v /var/run/docker.sock:/var/run/docker.sock \
  --restart always \
  ghcr.io/frierun/frierun:latest
```

## Podman

Podman is a Docker alternative. You can find installation instructions [here](https://podman.io/getting-started/installation).

However, Frierun requires a docker-compatible socket to run. Different OSs have different ways to do this.

### Linux

Depending on your distribution, the socket might be created but not activated. You can activate it by running:

```bash
systemctl start podman.socket
```

Refer to [installation instructions](https://github.com/containers/podman/blob/main/docs/tutorials/socket_activation.md) for your distribution if this command doesn't work.

```bash
podman run -d \
  --name frierun \
  -p 8080:8080 \
  -v frierun-config:/App/Frierun \
  -v /run/podman/podman.sock:/var/run/docker.sock \
  --restart always \
  ghcr.io/frierun/frierun:latest
```

### Windows

Podman on Windows creates the docker socket during the [installation](https://github.com/containers/podman/blob/main/docs/tutorials/podman-for-windows.md). 

```bash
podman run -d \
  --name frierun \
  -p 8080:8080 \
  -v frierun-config:/App/Frierun \
  -v /var/run/docker.sock:/var/run/docker.sock \
  --restart always \
  ghcr.io/frierun/frierun:latest
```

## Android

See [Termux documentation](/docs/Termux.md)

# Usage

Open your browser and go to `http://localhost:8080`.

Some packages require providers to be installed first, providers also add features to other applications. See the list below.

## Providers
- `traefik` provides the ability to route http traffic from one port to many containers using domain names.
- `static-zone` provides `traefik` with a static domain name. For local environments the `localhost` domain can be used.
  It would create subdomains like `app.localhost` for each application.
- `docker` is installed by default. It provides the ability to run applications in containers.
- `mysql`, `mariadb`, `postgresql` run in containers and provide other applications with access to separate databases.
- `cloudflare-tunnel` allows to expose applications to the internet using Cloudflare tunnels. See the [Cloudflare documentation](Docs/Cloudflare.md) for more details.
 
The `static-domain` provider is installed by default. But we also recommend to install `traefik` as well to make applications accessible by domain name.

# Screenshots

Homepage with some packages installed:
![Homepage](/Docs/Screenshot1.jpg?raw=true "Homepage")

Installing a new package:
![Install](/Docs/Screenshot2.jpg?raw=true "Install")

# Contributing

All contributions are welcome! You can create an [issue](https://github.com/frierun/frierun/issues) or a [pull request](https://github.com/frierun/frierun/pulls).

There is an indepth guide on how to [create a package](Docs/Package.md).