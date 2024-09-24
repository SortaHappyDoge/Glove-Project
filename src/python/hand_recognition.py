import cv2
import mediapipe as mp

# Initialize MediaPipe utilities and Webcan
mp_pose = mp.solutions.pose
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
cap = cv2.VideoCapture(0)


def main():
    0


# Initialize Pose and Hands models
with mp_pose.Pose(static_image_mode=False, min_detection_confidence=0.4, min_tracking_confidence=0.7) as pose, \
     mp_hands.Hands(static_image_mode=False, max_num_hands=2, min_detection_confidence=0.4, min_tracking_confidence=0.7) as hands:


    # Perform pose and hands detection !!! Use only one of those functions at the same time !!!
    def process_pose_and_hands():
        global pose_results, hand_results
        image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        pose_results = pose.process(image_rgb)
        hand_results = hands.process(image_rgb)
    # Perform hands detection
    def process_hands():
        global hand_results
        image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        hand_results = hands.process(image_rgb)
    # Perform pose detection
    def process_pose():
        global pose_results
        image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        pose_results = pose.process(image_rgb)


    # Draw pose landmarks on the frame if detection is successful
    def draw_pose():
        if pose_results.pose_landmarks:
            mp_drawing.draw_landmarks(
                frame,
                pose_results.pose_landmarks,
                mp_pose.POSE_CONNECTIONS,
                mp_drawing.DrawingSpec(color=(255, 0, 0), thickness=2, circle_radius=2),  # Pose landmark circles
                mp_drawing.DrawingSpec(color=(0, 255, 0), thickness=2, circle_radius=2))   # Pose connection lines
    # Draw hand landmarks on the frame if hands are detected
    def draw_hands():
        if hand_results.multi_hand_landmarks:
            for hand_landmarks in hand_results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(
                    frame,
                    hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing.DrawingSpec(color=(0, 0, 255), thickness=2, circle_radius=2),  # Hand landmark circles
                    mp_drawing.DrawingSpec(color=(255, 255, 0), thickness=2, circle_radius=2))  # Hand connection lines


    while cap.isOpened():
        success, frame = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        process_pose_and_hands()
        draw_pose()
        draw_hands()

        # Display the frame with pose and hand landmarks
        cv2.imshow('MediaPipe Detection Results', frame)

        # Break loop on 'q' key press
        if cv2.waitKey(5) & 0xFF == 27:
            break

# Release the webcam and close the OpenCV window
cap.release()
cv2.destroyAllWindows()

if __name__ == "__main__":
    main()