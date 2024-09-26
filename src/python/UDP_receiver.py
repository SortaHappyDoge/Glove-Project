# udp_client.py
import socket

# Define the client's IP and port (listening port)
client_address = ("127.0.0.1", 6789)  # Bind to all available IPs on port 6789

# Create a UDP socket
udp_client_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind the socket to the client address
udp_client_socket.bind(client_address)

print("Client is listening for UDP messages...")

try:
    while True:
        # Receive the message and sender's address
        data, sender_address = udp_client_socket.recvfrom(1024)  # Buffer size of 1024 bytes
        message = data.decode()
        print(f"Received message: '{message}' from {sender_address[0]}:{sender_address[1]}")
except KeyboardInterrupt:
    print("Client closed.")
finally:
    udp_client_socket.close()
