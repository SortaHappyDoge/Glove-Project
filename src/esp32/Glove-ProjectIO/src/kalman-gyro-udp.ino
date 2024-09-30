#include <Arduino.h>
#include <FastIMU.h>
#include <Wire.h>
#include <WiFi.h>
#include <WiFiUdp.h>
//#include <EEPROM.h>

const char* ssid = "TurkTelekom_Z7FMA";
const char* password = "3f17Gm9s61";
const bool custom_network = false;


const char* connection_ip = "192.168.1.37";
int port = 8000;
WiFiUDP client;
uint8_t IMUid = 0;
size_t message_size = 1024;

struct OutputData
{
    float roll;
    float pitch;
    uint8_t id;
}; OutputData output;
//#define EEPROM_SIZE 1024


#define IMU_ADDRESS 0x68    //I2C address for the IMU
#define PERFORM_CALIBRATION //Comment to disable startup calibration
MPU6050 IMU;                //Reference the MPU6050 class for use in code


calData calib = { 0 };      //Calibration data will be stored here, you can manually input calibration data in here
AccelData accelData;        //Acceleration data will be storeed here
GyroData gyroData;          //Gyro data will be storeed here
//MagData magData;            //Magnetometer data will be stored here, comment if no magnetometer
struct SavedData
{
    calData calibData;
}; SavedData savedData;


TaskHandle_t Core0Task;

void setup(){
    Wire.begin(); Wire.setClock(400000);    //Started I2C protocol, max speed 400khz
    Serial.begin(115200); while (!Serial){ ; }

    int err = IMU.init(calib, IMU_ADDRESS); //Initialize IMU, any error with initialization will be stored in err
    if(err != 0){ Serial.println("Error initializing IMU"); Serial.println(err); while(true){ ; } }

    #ifdef PERFORM_CALIBRATION
        calibrate_imu();
        IMU.init(calib, IMU_ADDRESS);
        savedData.calibData = calib;

        uint8_t byteArray[sizeof(OutputData)];
        memcpy(byteArray, &savedData, sizeof(OutputData));

        //Delayed: Brownout detector was triggered problem.
        //EEPROM.put(IMUid, byteArray); //TODO: Implement reading the caldata from eeprom
        
    #endif
    err = IMU.setGyroRange(500);      //Use these to set the range, smaller the range better the accuracy
    err = IMU.setAccelRange(8);       //Gyro ±250, ±500, ±1000, and ±2000 °/sec (dps), Accelerometer ±2g, ±4g, ±8g, ±16g
    if (err != 0) { Serial.print("Error Setting range: "); Serial.println(err); while (true){ ; } }
    delay(20);

    connect_to_network();

    xTaskCreatePinnedToCore(Core0loop, "Core 0 Loop", 10000, NULL, 0, &Core0Task, 0);
}


// Constants
double dt = 0.01;         // Time step (10 ms)
double Q_theta = 1e-3;    // Process noise variance for angle
double Q_omega = 1e-3;    // Process noise variance for angular velocity
double R = 1e-3;          // Measurement noise variance (accelerometer noise)

// Variables for Kalman Filter
double theta = 0;         // Estimated angle
double omega = 0;         // Estimated angular velocity
double P_theta = 1;       // Uncertainty in angle
double P_omega = 1;       // Uncertainty in angular velocity

double theta1 = 0;         // Estimated angle
double omega1 = 0;         // Estimated angular velocity
double P_theta1 = 1;       // Uncertainty in angle
double P_omega1 = 1;       // Uncertainty in angular velocity

float *accelerometer_angle_measurement(float accelX, float accelY, float accelZ){
    float *out_array = new float[2];
    
    out_array[0] = 180 * atan2(accelX, sqrt(accelY*accelY + accelZ*accelZ))/PI;
    out_array[1] = 180 * atan2(accelY, sqrt(accelX*accelX + accelZ*accelZ))/PI;

    return out_array;
}

void kalman_predict(double &theta, double &omega, double &P_theta, double &P_omega, double gyro_data, double dt){
    //Convert from °/s to r/s
    gyro_data = gyro_data * (PI / 180);

    // Predict new angle using the gyro
    theta = theta + omega * dt;
    
    // Angular velocity remains constant during prediction
    omega = omega;
    
    // Update the error covariance (uncertainty in prediction)
    P_theta = P_theta + Q_theta;
    P_omega = P_omega + Q_omega;
}

void kalman_update(double &theta, double &omega, double &P_theta, double &P_omega, double accel_angle){
    accel_angle = accel_angle * (PI / 180);
    
    // Measurement residual (difference between predicted angle and measured angle)
    double y = accel_angle - theta;
    
    // Kalman Gain
    double K = P_theta / (P_theta + R);
    
    // Update the angle estimate with the accelerometer data
    theta = theta + K * y;
    
    // Update error covariance
    P_theta = (1 - K) * P_theta;
    
    // Angular velocity is updated based on the gyroscope, so we don't update it here
}


void Core0loop(void * parameter){
    for(;;){
            float *tmp =accelerometer_angle_measurement(accelData.accelX, accelData.accelY, accelData.accelZ);
            
            //Roll
            kalman_predict(theta, omega, P_theta, P_omega, gyroData.gyroX, dt);
            kalman_update(theta, omega, P_theta, P_omega, tmp[1]);
            double theta_deg = theta * 180.0 / M_PI;
            Serial.print(theta_deg);
            output.roll = theta_deg;
            Serial.print("\t");

            //Pitch
            kalman_predict(theta1, omega1, P_theta1, P_omega1, gyroData.gyroY, dt);
            kalman_update(theta1, omega1, P_theta1, P_omega1, tmp[0]);
            theta_deg = theta1 * 180.0 / M_PI;
            Serial.println(theta_deg);
            output.pitch = theta_deg;


            delete[] tmp;

            delay(10);
    }
}

void loop(){
    IMU.update();
    IMU.getAccel(&accelData); 
    IMU.getGyro(&gyroData);
    run_client(output);
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

void connect_to_network(){
    WiFi.begin(ssid, password);

    while (WiFi.status() != WL_CONNECTED){
        delay(500);
        Serial.println("...");
    }

    Serial.print("WiFi connected with IP:");
    Serial.println(WiFi.localIP());
}

void run_client(OutputData message)
{
    message.id = IMUid;

    client.beginPacket(connection_ip, port);
    client.write((uint8_t *)&message, sizeof(message));
    client.endPacket();

    //Serial.println("Data transmitted...");
}

// Reserved for kalman








//