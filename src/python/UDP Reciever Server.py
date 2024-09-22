import socket
from dataclasses import dataclass
from struct import unpack

server_ip = socket.gethostbyname(socket.getfqdn())
port = 8000

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
            var0, var1, var2, var3, var4, var5, var6 = unpack("2f", request)
            
            with open("C:/Users/Akif/Desktop/ESP Projects/Python Socket Server/Output.txt", "r+") as text_file:
                text_file.write(str(var0)  + " " + str(var1))
            print(str(var0)  + " " + str(var1))
        except Exception as e:
            print(f"Error: {e}")

   
    
    

if __name__ == "__main__":
    main()
