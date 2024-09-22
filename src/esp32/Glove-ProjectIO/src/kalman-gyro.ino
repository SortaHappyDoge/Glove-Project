#include <Arduino.h>
#include <FastIMU.h>
#include <Wire.h>



#define IMU_ADDRESS 0x68    //I2C address for the IMU
#define PERFORM_CALIBRATION //Comment to disable startup calibration
MPU6050 IMU;                //Reference the MPU6050 class for use in code


calData calib = { 0 };      //Calibration data will be stored here, you can manually input calibration data in here
AccelData accelData;        //Acceleration data will be storeed here
GyroData gyroData;          //Gyro data will be storeed here
//MagData magData;            //Magnetometer data will be stored here, comment if no magnetometer


float kalman_angle_roll = 0, kalman_uncertainty_angle_roll = 2 * 2;
float kalman_angle_pitch = 0, kalman_uncertainty_angle_pitch = 2 * 2;
float kalman_1D_output[] = {0, 0};
float kalman_filtered_output[] = {0, 0};    // Stores the output given by the kalman filter (pitch and roll)


TaskHandle_t Core0Task;

void setup(){
    Wire.begin(); Wire.setClock(400000);    //Started I2C protocol, max speed 400khz
    Serial.begin(115200); while (!Serial){ ; }

    int err = IMU.init(calib, IMU_ADDRESS); //Initialize IMU, any error with initialization will be stored in err
    if(err != 0){ Serial.println("Error initializing IMU"); Serial.println(err); while(true){ ; } }

    #ifdef PERFORM_CALIBRATION
        calibrate_imu();
        IMU.init(calib, IMU_ADDRESS);
    #endif
    err = IMU.setGyroRange(500);      //Use these to set the range, smaller the range better the accuracy
    err = IMU.setAccelRange(8);       //Gyro ±250, ±500, ±1000, and ±2000 °/sec (dps), Accelerometer ±2g, ±4g, ±8g, ±16g
    if (err != 0) { Serial.print("Error Setting range: "); Serial.println(err); while (true){ ; } }
    delay(20);


    xTaskCreatePinnedToCore(Core0loop, "Core 0 Loop", 10000, NULL, 0, &Core0Task, 0);
}

void Core0loop(void * parameter){
    for(;;){
        if(IMU.dataAvailable()){
            calculate_kalman_output();
        }
    }
}

void loop(){
    IMU.update();
    IMU.getAccel(&accelData); 
    // Serial.print(accelData.accelX); Serial.print("\t"); Serial.print(accelData.accelY); Serial.print("\t"); Serial.print(accelData.accelZ); Serial.print("\t");
    IMU.getGyro(&gyroData);
    // Serial.print(gyroData.gyroX); Serial.print("\t"); Serial.print(gyroData.gyroY); Serial.print("\t"); Serial.println(gyroData.gyroZ);
    
    calculate_kalman_output(); 
    
    Serial.print(kalman_filtered_output[0]); Serial.print("\t"); Serial.println(kalman_filtered_output[1]);
    
    delay(20);
}

void calibrate_imu(){
    Serial.println("Keep IMU level.");
    delay(5000);
    IMU.calibrateAccelGyro(&calib);
    Serial.println("Calibration done!");
    Serial.println("Accel biases X/Y/Z: ");
    Serial.print(calib.accelBias[0]);
    Serial.print(", ");
    Serial.print(calib.accelBias[1]);
    Serial.print(", ");
    Serial.println(calib.accelBias[2]);
    Serial.println("Gyro biases X/Y/Z: ");
    Serial.print(calib.gyroBias[0]);
    Serial.print(", ");
    Serial.print(calib.gyroBias[1]);
    Serial.print(", ");
    Serial.println(calib.gyroBias[2]);
    delay(1000);
}


void calculate_kalman_1D(float kalman_state, float kalman_uncertainty, float kalman_input, float kalman_measurement){
    kalman_state = kalman_state + (.004 * kalman_input);
    kalman_uncertainty = kalman_uncertainty + (.004*.004 * 4*4);
    float kalman_gain = kalman_uncertainty * 1/(1 * kalman_uncertainty + 3*3);
    kalman_state = kalman_state + kalman_gain * (kalman_measurement - kalman_state);
    kalman_uncertainty = (1 - kalman_gain) * kalman_uncertainty;
    
    kalman_1D_output[0] = kalman_state; kalman_1D_output[1] = kalman_uncertainty;
}

// Calculates the output with kalman filter applied, 
// value is returned to kalman_filter_output
void calculate_kalman_output(){
    float angle_roll = atan(accelData.accelY/sqrt(accelData.accelX*accelData.accelX + accelData.accelZ*accelData.accelZ)) * 1/(3.1416/180);
    float angle_pitch = -atan(accelData.accelX/sqrt(accelData.accelY*accelData.accelY + accelData.accelZ*accelData.accelZ)) * 1/(3.1416/180);
    
    calculate_kalman_1D(kalman_angle_roll, kalman_uncertainty_angle_roll, gyroData.gyroX, angle_roll);
    kalman_angle_roll = kalman_1D_output[0]; kalman_uncertainty_angle_roll = kalman_1D_output[1];

    calculate_kalman_1D(kalman_angle_pitch, kalman_uncertainty_angle_pitch, gyroData.gyroY, angle_pitch);
    kalman_angle_pitch = kalman_1D_output[0]; kalman_uncertainty_angle_pitch = kalman_1D_output[1];

    kalman_filtered_output[0] = kalman_angle_roll;
    kalman_filtered_output[1] = kalman_angle_pitch;
}