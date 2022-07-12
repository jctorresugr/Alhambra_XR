#!/usr/bin/python3
#-*-coding:utf8-*-

import socket
import time
import struct

self = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
self.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
host = "0.0.0.0"
port = 8080
self.bind((host, port))

self.listen(5)
print("server started and listening")

(clientsocket, address) = self.accept()
print(f"connected to: {address}")

while True:
    data   = "{'x': 42}"
    packer = struct.Struct(f">I{len(data)}s")
    packet = packer.pack(len(data), data.encode())

    clientsocket.send(packet)
    print(f"{data} sent. Received: {str(clientsocket.recv(128))}");
