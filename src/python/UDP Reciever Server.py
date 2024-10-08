import socket
from dataclasses import dataclass
from struct import unpack, pack

server_ip = socket.gethostbyname(socket.getfqdn())
port = 8000

simulation_ip = socket.gethostbyname(socket.getfqdn())
simulation_port = 1234

@dataclass
class State:
    pitch: float
    roll: float

def main():
    initialize_server() 
    
def initialize_server(): # initialize server
    print("Server initializing...\n")
    global server
    server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # create a socket object
    server.bind((server_ip, port))
    
    #server.listen(0) # start listening to clients, no queue limit
    print(f"Listening on {server_ip}:{port}")
    run_server()

    
def run_server(): # the server loop, end this to stop the server...
    while(True):
        try:
            request, address = server.recvfrom(1024)
            var0, var1, var2 = unpack("2fi", request)
            
            
            #message = pack("2f", var0, var1)
            message = str(var0) + " , " + str(var1)

            try:
                server.sendto(message.encode("utf-8"), (simulation_ip, simulation_port))
            except Exception as e:
                print(f"Error: {e}")
                e = e
            print(str(var0)  + " " + str(var1))

        except Exception as e:
            print(f"Error: {e}")

   
    
    

if __name__ == "__main__":
    main()
