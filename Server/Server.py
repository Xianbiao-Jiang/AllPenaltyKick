# -*- coding: utf-8 -*-
"""
Created on Sun Jan 29 23:58:22 2023

@author: xianb
"""

# -*- coding: utf-8 -*-
"""
Created on Fri Jan 27 23:51:06 2023

@author: xianb
"""

# multiconn-server.py
#UDP
import UdpComms as U

#TCP
import sys
import socket
import selectors
import types


sockUDP = U.UdpComms(udpIP="192.168.3.4", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)

i = 0

IPList = set()

clientIP = sockUDP.clientIP # read data


#TCP

TCPIPList = set()

def accept_wrapper(sock):
    conn, addr = sock.accept()  # Should be ready to read
    print(f"Accepted connection from {addr}")
    TCPIPList.add(conn)
    conn.setblocking(False)
    data = types.SimpleNamespace(addr=addr, inb=b"", outb=b"")
    events = selectors.EVENT_READ | selectors.EVENT_WRITE
    sel.register(conn, events, data=data)
    
    
def service_connection(key, mask):
    sock = key.fileobj
    data = key.data
    if mask & selectors.EVENT_READ:
        recv_data = sock.recv(1024)  # Should be ready to read
        if recv_data:
            data.outb += recv_data
            print(f"message from {data.addr}")
        else:
            print(f"Closing connection to {data.addr}")
            sel.unregister(sock)
            sock.close()
    if mask & selectors.EVENT_WRITE:
        if data.outb:
            print(f"Echoing {data.outb!r} to {data.addr}")
            for each in TCPIPList:
                if each != sock:
#                    print(data)
                    sent = each.send(data.outb)
                    data.outb = data.outb[sent:]
             # print new received data
#            sent = sock.send(data.outb)  # Should be ready to write
#            data.outb = data.outb[sent:]



sel = selectors.DefaultSelector()

# ...

host, port = "192.168.3.4", 25001
lsock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
lsock.bind((host, port))
lsock.listen()
print(f"Listening on {(host, port)}")
lsock.setblocking(False)
sel.register(lsock, selectors.EVENT_READ, data=None)

try:
    while True:
        #UDP
        
        i += 1
 #       print(i)

        dataUDP = sockUDP.ReadReceivedData() # read data
        
        clientIP = sockUDP.clientIP # read data



        if dataUDP != None: # if NEW data has been received since last ReadReceivedData function call
            IPList.add(clientIP)
            print(IPList)
            if "HandPosition" in dataUDP: 
#            print(data) # print new received data
                for each in IPList:
                    if each != clientIP:
                        sockUDP.SendData2(dataUDP, each)
                    print(dataUDP) # print new received data
            if "BallPosition" in dataUDP: 
#            print(data) # print new received data
                for each in IPList:
                    if each != clientIP:
                        sockUDP.SendData2(dataUDP, each)
#                    print(dataUDP) # print new received data        
            else:
                    sockUDP.SendData2("Accept", clientIP)
                
        
                
        #TCP        
        events = sel.select(timeout=None)
        for key, mask in events:
            if key.data is None:
                accept_wrapper(key.fileobj)
            else:
                service_connection(key, mask)
except KeyboardInterrupt:
    print("Caught keyboard interrupt, exiting")
finally:
    sel.close()





