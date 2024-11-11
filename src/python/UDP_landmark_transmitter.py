#from sys import getsizeof
import socket
import body_recognition as br
import cv2
cap = cv2.VideoCapture(0)

address_to_send = "localhost"
port_to_send = 8000
server_address = (address_to_send, port_to_send)  # 'localhost' is the IP for local testing, and port 6789 is chosen arbitrarily

    
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
        hand_result = br.hands.process(image_rgb)
        # Draw and Display the frame with pose and hand landmarks
        br.draw_hands(image_bgr, hand_result)
        cv2.imshow('MediaPipe Detection Results', image_bgr)

        # Sends any detected hand's landmarks with the format: [[(hand_id, landmark_id, x, y, z), ...],[...]]
        # Recommended buffer size is 3584 bytes (3584 = 2^11 * 2^10 * 2^9)
        # Landmarks of 1 hand ~1700 bytes
        # Landmarks of 2 hands ~3400 bytes
        message = str((br.get_hand_landmarks(hand_result)))
        udp_server_socket.sendto(message.encode(), server_address)
        #print(f"Message sent:{len(message)} bytes")
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