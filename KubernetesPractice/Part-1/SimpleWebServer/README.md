The simple web server serves some files


# General information
The webserver serves the following endpoints:
- / : show summary page
- /create_encoded_file : To create your own secret file
- /encoded : Get the encoded file
- /decoded : Get the decoded file

The service uses the following environment variables:
- SECRET_KEY : The secret key to use to encode / decode. This currently only takes base64 encoded keys
- SECRET_PATH : Where is the secret located with respect to the root? (defaults to secret.bin)

A Docker file has been provided for reference to get all requirements installed.

# Task
Your task is to create the kubernetes spec to run the webserver
- Get the code into the container
- Pass the secret to the application
- Trigger the secret file generation
- Get the decoded value using the application

## Secret key
For the solutions the secret key "pon8jK17OUFMEJIhaK_DMzLQDwC6aLkfWASwfBpUbpI=" is used (remove quotes!)


## Solution options
1. Create a configmap containing the server and requirements file
   1. Not traditional, but no jump host needed!
   2. How? Modify server.yaml, deploy server-configmap.yaml
2. Mount files container (e.g. on minikube)
   1. How? Change server.yaml to mount the directory in the right place(s) (and add the environment variable)
3. Create docker image and use it (can be used locally, otherwise requires a docker repository)
   1. How? Use Dockerfile, Alter server.yaml
