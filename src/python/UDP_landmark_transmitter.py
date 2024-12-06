#from sys import getsizeof
import socket
import body_recognition as br
import cv2
import time
from struct import pack
cap = cv2.VideoCapture(0)


simulation_address = socket.gethostbyname(socket.getfqdn()) #socket.gethostbyname(socket.getfqdn())
simulation_port = 8000
server_address = (simulation_address, simulation_port)  # 'localhost' is the IP for local testing, and port 6789 is chosen arbitrarily
message_id = 2 # the message identifier used for differentiating data sent to "UDP Reciever Server.py"


def main():
    # Create a UDP socket
    udp_server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    while cap.isOpened():
        success, image_bgr = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        image_bgr = cv2.flip(image_bgr, 1) # Flip the camera if needed

        # Turn BGR image to RGB image
        image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)
        hand_result = br.hands.process(image_rgb)
        pose_result = br.pose.process(image_rgb)
        # Draw and Display the frame with pose and hand landmarks
        br.draw_hands(image_bgr, hand_result)
        br.draw_pose(image_bgr, pose_result)
        cv2.imshow('MediaPipe Detection Results', image_bgr)

        # Sends any detected hand's landmarks with the format: [[(hand_id, landmark_id, x, y, z), ...],[...]]
        # Recommended buffer size is 3584 bytes (3584 = 2^11 * 2^10 * 2^9)
        # Landmarks of 1 hand ~1700 bytes
        # Landmarks of 2 hands ~3400 bytes
        message = br.get_hand_landmarks(hand_result)

        # Add pose data to display location of hands instead of just the position of hands
        if br.get_pose_landmarks(pose_result): 
            message.append(br.get_pose_landmarks(pose_result)[15])
            message.append(br.get_pose_landmarks(pose_result)[16])
        else:                                  
            message.append((2.0, 15.0, -0.5, 0.0, 0.0))
            message.append((2.0, 16.0, 0.5, 0.0, 0.0))
        
        message_to_send = []

        # Convert the message into a single dimentional list to send over UDP by packing
        for landmark in message:
            for value in landmark:
                message_to_send.append(value)

        
        buffer = pack(f'{len(message_to_send)}f', *message_to_send) 
        
        # Turn the buffer into byte array to add identification byte to the beginnig
        byte_buffer = bytearray(buffer)
        byte_buffer.insert(0, message_id)

        udp_server_socket.sendto(byte_buffer, server_address)  # Send the message to the specified address
        print(f"Sent :{message}")
        

        """
        if len(message)>0:
            #message.insert(0, bytes(message_id[0])) # Changes the first byte of the list with the message_id as an identifier
            
            
            buff = bytearray([])
            buff.extend((message_id).to_bytes())
            
            if len(message) == 1:
                landmarks = message[0]                  # !!! The variable -message- is string this just takes a single character
                for i in landmarks:
                    buff.extend(pack("2i3f", *i))
                    
            elif len(message) == 2:
                landmarks = message[0] + message[1]     # !!! The variable -message- is string this just takes a single character
                for i in landmarks:
                    buff.extend(pack("2i3f", *i))
                    
            else:
                print("message size error")
                return
            
            #print(len(buff))
            #print(buff)
        """

        # Break loop on 'esc' key press
        if cv2.waitKey(1) & 0xFF == 27:
            # Release the webcam and close the OpenCV window
            udp_server_socket.close()
            cap.release()
            cv2.destroyAllWindows()
            break


"""
def socket_test_function():
    # Create a UDP socket
    udp_server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    # Variable to send
    message = "Hello from the server!"
    
    try:
        while True:
            # Send the message to the specified address
            udp_server_socket.sendto(message.encode(), server_address)
            print(f"Message sent: {message}")            
    except KeyboardInterrupt:
        print("Server closed.")
    finally:
        udp_server_socket.close()


def size_test_function():
    while cap.isOpened():
        success, image_bgr = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)
        pose_result = br.pose.process(image_rgb)
        hand_result = br.hands.process(image_rgb)
        message = (br.get_pose_landmarks(pose_result), br.get_hand_landmarks(hand_result))
        print(getsizeof(message))
        br.draw_pose(image_bgr, pose_result)
        br.draw_hands(image_bgr, hand_result)

        # Display the frame with pose and hand landmarks
        cv2.imshow('MediaPipe Detection Results', image_bgr)
        
        # Send the message to the specified address
        print(f"Message sent:")
                
        # Break loop on 'esc' key press
        if cv2.waitKey(1) & 0xFF == 27:
            # Release the webcam and close the OpenCV window
            cap.release()
            cv2.destroyAllWindows()
            break
"""


if __name__ == "__main__":
    main()