## About the Project

Frierun is an open-source tool for easy one-click deployment of applications to your servers or clouds.

## Current state of the project

This project is in the early stages of development. It is not recommended to use it in production.

## Installing

Run it through the docker container

```bash
docker run --name frierun -p 8080:8080 -v /var/run/docker.sock:/var/run/docker.sock ghcr.io/frierun/frierun:main
```

## Usage

Open your browser and go to `http://localhost:8080`.

Install `traefik` first to route http traffic to other containers.
