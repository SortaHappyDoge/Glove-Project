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
}

void loop(){
    IMU.update();
    IMU.getAccel(&accelData); 
    Serial.print(accelData.accelX); Serial.print("\t"); Serial.print(accelData.accelY); Serial.print("\t"); Serial.print(accelData.accelZ); Serial.print("\t");
    IMU.getGyro(&gyroData);
    Serial.print(gyroData.gyroX); Serial.print("\t"); Serial.print(gyroData.gyroY); Serial.print("\t"); Serial.println(gyroData.gyroZ);
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