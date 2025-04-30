#!/usr/bin/env python
import os

from flask import Flask, Response
from cryptography.fernet import Fernet

SECRET_KEY = os.environ.get('SECRET_KEY')
ENCODED_FILE_PATH = os.environ.get('SECRET_PATH', 'secret.bin')

app = Flask(__name__)


@app.route("/create_encoded_file")
def create_encoded_file():
    """To create your own secret; For the minikube / local deployments."""
    # To create your own secret key: Fernet.generate_key()

    if not SECRET_KEY:
        return Response(f'Error: Secret not configured', 500)

    secret_data = Fernet(SECRET_KEY).encrypt(
        "Great, you decoded your own secret!".encode('utf8')
    )
    with open(ENCODED_FILE_PATH, 'wb') as file:
        file.write(secret_data)
    return f"You can find the secret file at {ENCODED_FILE_PATH}"


@app.route("/")
def hello_world():
    return f"<p>The server is online; " \
           f"secret?: {SECRET_KEY is not None}; " \
           f"Secret file? {os.path.exists(ENCODED_FILE_PATH)} </p>"


@app.route("/encoded")
def raw_file():
    if not os.path.exists(ENCODED_FILE_PATH):
        return Response(f'Error: encoded file not found at {ENCODED_FILE_PATH}', 500)

    with open(ENCODED_FILE_PATH, 'rb') as file:
        return file.read()


@app.route("/decoded")
def decoded_file():
    raw_file_content = raw_file()
    if isinstance(raw_file_content, Response):
        return raw_file_content  # error message

    if not SECRET_KEY:
        return Response(f'Error: Secret not configured', 500)

    return Fernet(SECRET_KEY).decrypt(raw_file_content).decode('utf8')


if __name__ == '__main__':
    # Server is now accessible via port forwarding of port 6677
    app.run('0.0.0.0', 6677)
