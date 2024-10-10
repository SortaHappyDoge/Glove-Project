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

def handle_connection():
    buffer, address = server.recvfrom(1024)
    print(buffer[0])
    match buffer[0]:   # will change this to use more 
        case 1:     # data recieved from the esp32, default 3 floats (12 bytes)
            type, pitch, roll, id = unpack("B3f", buffer)
            print(pitch) # placeholder
        case 2:     # data recieved from hand recognition model, default NOT YET ASSIGNED
            print() # placeholder
    
def run_server(): # the server loop, end this to stop the server...
    while(True):
        handle_connection()

        """
        try:
            request, address = server.recvfrom()
            
            
            try:
                server.sendto(message.encode("utf-8"), (simulation_ip, simulation_port))
            except Exception as e:
                print(f"Error: {e}")
                e = e
            print(str(var0)  + " " + str(var1))

        except Exception as e:
            print(f"Error: {e}")
        """
        

   
    
    

if __name__ == "__main__":
    main()
