#from sys import getsizeof
import socket
import body_recognition as br
import cv2
cap = cv2.VideoCapture(0)

simulation_address = socket.gethostbyname(socket.getfqdn())
simulation_port = 8000
server_address = (simulation_address, simulation_port)  # 'localhost' is the IP for local testing, and port 6789 is chosen arbitrarily
message_id = b'\x01' # the message identifier used for differentiating data sent to "UDP Reciever Server.py"
    
def main():
    # Create a UDP socket
    udp_server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    while cap.isOpened():
        success, image_bgr = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue
        # Turn BGR image to RGB image
        image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)

        pose_result = br.pose.process(image_rgb)
        hand_result = br.hands.process(image_rgb)
        # Sends any detected hand's landmarks with the format: [(hand_id, landmark_id, x, y, z), ...]
        message = (br.get_hand_landmarks(hand_result))

        br.draw_pose(image_bgr, pose_result)
        br.draw_hands(image_bgr, hand_result)

        # Display the frame with pose and hand landmarks
        cv2.imshow('MediaPipe Detection Results', image_bgr)
        
        if len(message)>0:
            message.insert(0, message_id[0]) # Changes the first byte of the list with the message_id as an identifier
            buff = str(message).removeprefix("[").removesuffix("]").encode()            
            udp_server_socket.sendto(buff, server_address)  # Send the message to the specified address
            #print(buff)
            #print(f"Message sent:{message}")


                
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