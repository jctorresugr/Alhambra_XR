#!/usr/bin/python3
#-*-coding:utf8-*-

import socket
import time
import struct
import matplotlib.image as mpimage
import base64
import numpy as np
import json

def sendJSON(clientsocket, data):
    packer = struct.Struct(f">I{len(data)}s")
    packet = packer.pack(len(data), data.encode())

    clientsocket.send(packet)

self = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
self.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
host = "0.0.0.0"
port = 8080
self.bind((host, port))

self.listen(5)
print("server started and listening")

(clientsocket, address) = self.accept()
print(f"connected to: {address}")


#Send a selection
data = """{
        'action': 'selection',
        'data': {
                'ids': [1,5,7]
            }
        }"""
sendJSON(clientsocket, data);

#Send a start annotation
tmp = mpimage.imread("img.png")
tmp = tmp*255
tmp = tmp.astype(np.int8)
img = np.zeros((tmp.shape[0], tmp.shape[1], 4), np.int8) #ARGB8888
img[:,:,1:] = tmp
img[:,:,0]  = 255
argb8888 = base64.b64encode(img.tobytes())
data = """{
            'action': 'annotation',
            'data': {
                'width': %d,
                'height': %d,
                'base64': '%s'
            }
       }""" % (img.shape[1], img.shape[0], argb8888.decode())
sendJSON(clientsocket, data);

while True:
    data = clientsocket.recv(4096)
    size   = struct.unpack(">I", data[:4])[0]
    string = data[4:4+size].decode()

    try:
        print(json.loads(string))
    except JSONDecodeError as e:
        print(e.message)

    time.sleep(1.0)
    continue
