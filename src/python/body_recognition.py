import mediapipe as mp


"""
!!!While using this as a library!!!
-process, pose or data functins take in a RGB type image
-print and return, pose or data functions take in the result of process functins
-all opencv related (e.g. taking video feed from camera) should be done on the script this will be used in
"""


# Initialize MediaPipe utilities and Webcan
mp_pose = mp.solutions.pose
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils


# Initialize Pose and Hands models
pose = mp_pose.Pose(static_image_mode=False, min_detection_confidence=0.6, min_tracking_confidence=0.8)
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=2, min_detection_confidence=0.6, min_tracking_confidence=0.8)


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
            

# Print pose and hand landmarks x,y,z coordinates with id
def print_hand_landmarks(hand_results):
    if hand_results.multi_hand_landmarks:
        for hand_no, hand_landmarks in enumerate(hand_results.multi_hand_landmarks):
            for id, landmark in enumerate(hand_landmarks.landmark):
                print(f"hand{hand_no}", id, landmark.x, landmark.y, landmark.z)
def print_pose_landmarks(pose_results):
    if pose_results.pose_landmarks:
        for id, landmark in enumerate(pose_results.pose_landmarks.landmark):
            print("pose", id, landmark.x, landmark.y, landmark.z)

# Temporary function for getting wrist coords of multi_hand_landmarks
"""def get_wrist_landmarks(hand_results):
    left_wrist, right_wrist = None, None
    if hand_results.multi_hand_landmarks and hand_results.multi_handedness:
        for handedness, hand_landmarks in zip(hand_results.multi_handedness, hand_results.multi_hand_landmarks):
            if handedness.classification[0].label == "Left":
                left_list = hand_landmarks.landmark[0]
                left_wrist = (2.0, float(0), left_list.x, left_list.y, left_list.z)

            if handedness.classification[0].label == "Right":
                right_list = hand_landmarks.landmark[0]
                right_wrist = (2.0, float(0), right_list.x, right_list.y, right_list.z)

            if left_wrist == None:
                return right_list
            if right_wrist == None:
                return left_wrist
            return left_wrist, right_wrist
    return None, None"""

def get_wrist_landmarks(hand_results, screen_x = 1, screen_y = 1):
    wrist_landmarks = []
    
    if hand_results.multi_hand_landmarks and hand_results.multi_handedness:
        # Set default wrist coordinates (0, 0, 0) in case hand is not detected
        left_wrist_detected = False
        right_wrist_detected = False
        
        for handedness, hand_landmarks in zip(hand_results.multi_handedness, hand_results.multi_hand_landmarks):
            # Get the wrist landmark (index 0)
            wrist = hand_landmarks.landmark[0]
            
            # Check if the hand is left or right and process the wrist landmark
            if handedness.classification[0].label == "Left":
                left_wrist_detected = True
                wrist_landmarks.append((1, 0, wrist.x * screen_x, wrist.y * screen_y, wrist.z))  # Left hand wrist
            elif handedness.classification[0].label == "Right":
                right_wrist_detected = True
                wrist_landmarks.append((0, 0, wrist.x * screen_x, wrist.y * screen_y, wrist.z))  # Right hand wrist
        
        # If the left wrist wasn't detected, add the default coordinates (0, 0, 0)
        if not left_wrist_detected:
            wrist_landmarks.append((0, 0, 0.0, 0.0, 0.0))  # Left wrist with default (0, 0, 0)

        # If the right wrist wasn't detected, add the default coordinates (0, 0, 0)
        if not right_wrist_detected:
            wrist_landmarks.append((1, 0, 0.0, 0.0, 0.0))  # Right wrist with default (0, 0, 0)

    return wrist_landmarks


# Return pose and hand landmarks x,y,z coordinates with id as float
def get_hand_landmarks(hand_results):
    landmarks = []
    if hand_results.multi_hand_landmarks and hand_results.multi_handedness:
        for handedness, hand_landmarks in zip(hand_results.multi_handedness, hand_results.multi_hand_world_landmarks):
            if handedness.classification[0].label == "Left": hand_no = 1
            else: hand_no = 0
            #if handedness.classification[0].label == "Right": hand_no = 0
            for id, landmark in enumerate(hand_landmarks.landmark):
                if hand_no == 1 or hand_no == 0:
                    landmarks.append((float(hand_no), float(id + (hand_no * 21)), landmark.x, landmark.y, landmark.z))
    return landmarks
def get_pose_landmarks(pose_results):
    landmarks = []
    if pose_results.pose_landmarks:
        for id, landmark in enumerate(pose_results.pose_world_landmarks.landmark):
            landmarks.append((2.0, float(id), landmark.x, landmark.y, landmark.z))
    return landmarks


def main():
    import cv2
    cap = cv2.VideoCapture(0)
    while cap.isOpened():
        success, image_bgr = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)
        pose_result = pose.process(image_rgb)
        hand_result = hands.process(image_rgb)
        print(get_pose_landmarks(pose_result))
        draw_pose(image_bgr, pose_result)
        draw_hands(image_bgr, hand_result)

        # Display the frame with pose and hand landmarks
        cv2.imshow('MediaPipe Detection Results', image_bgr)

        # Break loop on 'esc' key press
        if cv2.waitKey(1) & 0xFF == 27:
            # Release the webcam and close the OpenCV window
            cap.release()
            cv2.destroyAllWindows()
            break


if __name__ == "__main__":
    main()