import socket
from time import sleep

# Define the IP address and port to listen on
local_ip = "localhost" #socket.gethostbyname(socket.getfqdn())  # Listen on all available interfaces
local_port = 8000

# Create a UDP socket
udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind the socket to the IP and port
udp_socket.bind((local_ip, local_port))

print(f"Listening for UDP packets on {local_ip}:{local_port}...")

# Continuously receive data
try:
    while True:
        # Receive data from the socket
        data, addr = udp_socket.recvfrom(3584)  # Buffer size is 3584 bytes (3584 = 2^11 * 2^10 * 2^9)
        print(f"Received message: {data.decode()} from {addr}")
        #sleep(1.0)
except KeyboardInterrupt:
    print("\nServer interrupted. Exiting...")
finally:
    # Close the socket
    udp_socket.close()
