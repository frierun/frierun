## About the Project

Frierun is an open source Infrastructure as Code (IaC) tool.
It allows easy one-click installation and uninstallation, yet it is highly configurable via Dependency Injection.

It introduces the abstract package format for defining applications and their dependencies. 
These definitions can be written once and reused everywhere by everyone: 
packages can be deployed on different clouds and infrastructures without any changes.

## Current state of the project

This project is in the early stages of development. It is not recommended to use it in production. 
It is more like a proof of concept right now.

## Installing

Run it through the docker container

```bash
docker run --name frierun \
  -p 8080:8080 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  ghcr.io/frierun/frierun:main
```

## Usage

Open your browser and go to `http://localhost:8080`.

Install `traefik` first to route http traffic to other containers.

## Screenshots

Homepage with some packages installed:
![Homepage](/Docs/Screenshot1.jpg?raw=true "Homepage")

Installing a new package:
![Install](/Docs/Screenshot2.jpg?raw=true "Install")
