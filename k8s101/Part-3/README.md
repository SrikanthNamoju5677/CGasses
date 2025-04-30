# Introduction 
Training project at home for Kubernetes course at Alten 

# Getting Started
1.	Make sure the system you're running (locally or remote) meets the LINUX requirements from Part-1 and Part-2 and has at least 20gb of free space
2.  Use the docker-compose provided in ./repository to run a local container repository (see part 1 cheatsheet on how to run and push images) LINUX ONLY
3.	Switch directories to ./containers and continue from there
4.	Have fun learning!


--------------

# Build images (LINUX ONLY): 

## Check if registry is running:
curl http://localhost:5000/v2/

# but we need a little extra to be able to use it..

## change hostfile (use VIM, VI or NANO. Change command to fit the needs)
nano /etc/hosts

## add the following line:
localhost   registry.dev.svc.cluster.local

## set a fixed IP to resolve to locally 
sudo ifconfig lo:11 172.16.1.1
sudo vim /etc/docker/daemon.json
## add:
{
  "insecure-registries": ["registry.dev.svc.cluster.local:5000"]
}

sudo vim /etc/default/docker
## add:
DOCKER_OPTS="--config-file=/etc/docker/daemon.json"

## Build docker-compose images:
cd /path/to/repo
docker-compose build

## Push to the dummy registry:
docker-compose push



--------------

# WINDOWS
## using a local repository is currently untested. I recommend using the provided images from Part-2 (azure repository) in the yaml files
## This option is also available for LINUX users who cannot set up the local container repository.

--------------

# Run containers in Minikube: 

## switch directories to ./containers
- check the docker-compose and change for every entry if the -image- is pointing to the right repository (local build or remote from azure)


## start minikube
minikube start --cpus 2 --memory 2048 --insecure-registry registry.dev.svc.cluster.local:5000


# FOR USE WITH LOCAL REPOSITORY
## config upfront: use the fixed IP
export DEV_IP=172.16.1.1
minikube ssh "echo \"$DEV_IP       registry.dev.svc.cluster.local\" | sudo tee -a  /etc/hosts"

# Downloads
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Docker](https://www.docker.com/get-started)
- [Git for Windows](https://git-scm.com/download/win)
- [azure cli](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- [kompose](https://kompose.io/installation/)
- [lens6](https://k8slens.dev/)
- [minikube](https://minikube.sigs.k8s.io/docs/start/)