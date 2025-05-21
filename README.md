# About the Project

Frierun is an open-source project that aims to simplify the deployment and management of self-hosted applications.
It allows easy one-click installation and uninstallation of packages, yet it is highly configurable.

It introduces the abstract package format for defining applications and their dependencies. 
These definitions can be written once and reused everywhere by everyone: 
packages can be deployed on different clouds and infrastructures without any changes.

## Current state of the project

This project is in the early stages of development. It is not recommended to use it in production. 
It is more like a proof of concept right now.

# Installing

The easiest way to run Frierun is to use Docker. You might also use Podman instead if you prefer.

## Docker

Install Docker on your machine. You can find instructions [here](https://docs.docker.com/get-docker/).

Run Frierun container

```bash
docker run -d \
  --name frierun \
  -p 8080:8080 \
  -v frierun-config:/App/Frierun \
  -v /var/run/docker.sock:/var/run/docker.sock \
  --restart always \
  ghcr.io/frierun/frierun:main
```

## Podman

Podman is the Docker alternative. You can find install instructions [here](https://podman.io/getting-started/installation).

However, Frierun requires docker-compatible socket to run. Different OSs have different ways to do this.

### Linux

Depending on your distribution, the socket might be already created at `/usr/lib/systemd/user/podman.socket`.
Check [installation instructions](https://github.com/containers/podman/blob/main/docs/tutorials/socket_activation.md) if it doesn't exist.

```bash
podman run -d \
  --name frierun \
  -p 8080:8080 \
  -v frierun-config:/App/Frierun \
  -v /usr/lib/systemd/user/podman.socket:/var/run/docker.sock \
  --restart always \
  ghcr.io/frierun/frierun:main
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
  ghcr.io/frierun/frierun:main
```

# Usage

Open your browser and go to `http://localhost:8080`.

Providers adds features to other applications.

## Providers

- `traefik` - provides ability to route http traffic from one port to man containers using domain names.
- `static-domain` - provides `traefik` with static domain name. For local environments the `localhost` domain can be used.
  It would create subdomains like `app.localhost` for each application.
- `docker` - is installed by default. It provides ability to run applications in containers.
- `mysql`, `mariadb`, `postgresql` - runs in containers and provides other applications with access to separate databases.
 

Install `traefik` first to route http traffic to other containers.

# Screenshots

Homepage with some packages installed:
![Homepage](/Docs/Screenshot1.jpg?raw=true "Homepage")

Installing a new package:
![Install](/Docs/Screenshot2.jpg?raw=true "Install")

# Contributing

All contributions are welcome! You can create an [issue](https://github.com/frierun/frierun/issues) or a [pull request](https://github.com/frierun/frierun/pulls).