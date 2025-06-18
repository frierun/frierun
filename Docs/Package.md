# Changing and testing packages

You can update packages without having to rebuild the entire project. This is useful for testing changes in packages or 
when you want to use a different version of a package.

## Clone the repository

You only need the `Server/Packages` folder from the repository, but it might be easier to clone the full repository.
You can do it using GitHub Desktop or the command line:

```bash
git clone https://github.com/frierun/frierun.git
```

## Run the container 

You can run the container with the `-v` option to mount your local packages from the cloned repository.

```bash
docker run -d \
  --name frierun \
  -p 8080:8080 \
  -v frierun-config:/App/Frierun \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v /path/to/frierun/Server/Packages:/app/Packages \
  --restart always \
  ghcr.io/frierun/frierun:main
```
Change `/path/to/frierun/` to the path where you cloned the repository.

## Test your changes

You can add or modify packages in the `Server/Packages` folder. Docker requires you to restart the container to apply changes.

```bash
docker restart frierun
```