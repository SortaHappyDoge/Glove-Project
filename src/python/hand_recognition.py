import cv2
import mediapipe as mp

# Initialize MediaPipe utilities and Webcan
mp_pose = mp.solutions.pose
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
cap = cv2.VideoCapture(0)


# Initialize Pose and Hands models
with mp_pose.Pose(static_image_mode=False, min_detection_confidence=0.4, min_tracking_confidence=0.7) as pose, \
     mp_hands.Hands(static_image_mode=False, max_num_hands=2, min_detection_confidence=0.4, min_tracking_confidence=0.7) as hands:
    # Perform hands detection
    def process_hands(frame):  # Takes a RGB image
        #global hand_results
        hand_results = hands.process(frame)
        return hand_results
    # Perform pose detection
    def process_pose(frame):  # Takes a RGB image
        #global pose_results
        pose_results = pose.process(frame)
        return pose_results

    # Draw pose landmarks on the frame if detection is successful
    def draw_pose(frame, pose_results):
        if pose_results.pose_landmarks:
            mp_drawing.draw_landmarks(
                frame,
                pose_results.pose_landmarks,
                mp_pose.POSE_CONNECTIONS,
                mp_drawing.DrawingSpec(color=(255, 0, 0), thickness=2, circle_radius=2),  # Pose landmark circles
                mp_drawing.DrawingSpec(color=(0, 255, 0), thickness=2, circle_radius=2))   # Pose connection lines
    # Draw hand landmarks on the frame if hands are detected
    def draw_hands(frame, hand_results):
        if hand_results.multi_hand_landmarks:
            for hand_landmarks in hand_results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(
                    frame,
                    hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing.DrawingSpec(color=(0, 0, 255), thickness=2, circle_radius=2),  # Hand landmark circles
                    mp_drawing.DrawingSpec(color=(255, 255, 0), thickness=2, circle_radius=2))  # Hand connection lines
                
    def print_hand_landmarks(hand_results):
        if hand_results.multi_hand_landmarks:
            for hand_landmarks in hand_results.multi_hand_world_landmarks:
                print(hand_landmarks)

    def main():
        while cap.isOpened():
            success, image_bgr = cap.read()
            if not success:
                print("Ignoring empty camera frame.")
                continue    
            
            image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)
            pose_result = process_pose(image_bgr)
            hand_result = process_hands(image_rgb)
            draw_pose(image_bgr, pose_result)
            draw_hands(image_bgr, hand_result)
            print_hand_landmarks(hand_result)
    
            # Display the frame with pose and hand landmarks
            cv2.imshow('MediaPipe Detection Results', image_bgr)
    
            # Break loop on 'q' key press
            if cv2.waitKey(5) & 0xFF == 27:
                # Release the webcam and close the OpenCV window
                cap.release()
                cv2.destroyAllWindows()


if __name__ == "__main__":
    main()