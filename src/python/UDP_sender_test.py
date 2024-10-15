import socket
from time import sleep
# Define the server address and port
server_ip = "127.0.0.1" #socket.gethostbyname(socket.getfqdn())
server_port = 8000
server_address = (server_ip, server_port)  # Replace with the target IP address and port number

# Create a UDP socket
udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Message to be sent
message = "Hello, UDP!"

try:
    while True:
        # Send the message
        udp_socket.sendto(message.encode(), server_address)
        print(f"Message sent to {server_address}")
        sleep(1.0)
except KeyboardInterrupt:
    print("server closing")
finally:
    # Close the socket
    udp_socket.close()
