import socket
import time
import body_recognition as br
import cv2
cap = cv2.VideoCapture(0)
localIP = 1 

    
def main():
    server_address = ('localhost', 6789)  # 'localhost' is the IP for local testing, and port 6789 is chosen arbitrarily

    # Create a UDP socket
    udp_server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    while cap.isOpened():
        success, image_bgr = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue
    
        image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)
        pose_result = br.process_pose(image_rgb)
        hand_result = br.process_hands(image_rgb)
        message = br.get_pose_landmarks(hand_result)
        br.draw_pose(image_bgr, pose_result)
        br.draw_hands(image_bgr, hand_result)

        # Display the frame with pose and hand landmarks
        cv2.imshow('MediaPipe Detection Results', image_bgr)
        
        try:
            while True:
                # Send the message to the specified address
                udp_server_socket.sendto(message.encode(), server_address)
                print(f"Message sent:")
                
                # Wait for a few seconds before sending the next message
                time.sleep(2)
        except KeyboardInterrupt:
            print("Server closed.")
        finally:
            udp_server_socket.close()

        # Break loop on 'esc' key press
        if cv2.waitKey(1) & 0xFF == 27:
            # Release the webcam and close the OpenCV window
            cap.release()
            cv2.destroyAllWindows()
            break

def test_function():
    server_address = ('localhost', 6789)  # 'localhost' is the IP for local testing, and port 6789 is chosen arbitrarily

    # Create a UDP socket
    udp_server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    # Variable to send
    message = "Hello from the server!"
    
    try:
        while True:
            # Send the message to the specified address
            udp_server_socket.sendto(message.encode(), server_address)
            print(f"Message sent: {message}")
            
            # Wait for a few seconds before sending the next message
            time.sleep(2)
    except KeyboardInterrupt:
        print("Server closed.")
    finally:
        udp_server_socket.close()


if __name__ == "__main__":
    main()