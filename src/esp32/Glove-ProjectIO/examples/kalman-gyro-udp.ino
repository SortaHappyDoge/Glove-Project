#include <Arduino.h>
#include <FastIMU.h>
#include <Wire.h>
#include <WiFi.h>
#include <WiFiUdp.h>
//#include <EEPROM.h>

const char* ssid = "TurkTelekom_Z7FMA";
const char* password = "3f17Gm9s61";
const bool custom_network = false;


const char* connection_ip = "192.168.1.48";
int port = 8000;
WiFiUDP client;
uint8_t IMUid = 0;
size_t message_size = 12;

struct OutputData
{
    uint8_t pass = 1;
    float roll;
    float pitch;
    float id;
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

//Reserved for kalman variables
const float dt = 0.01;
const float Q_angle = 0.001;
const float Q_gyro = 0.003;
const float R_angle = 0.03;

float x_roll = 0.0;
float x_pitch = 0.0;
float P_roll[2][2] = {{1, 0}, {0, 1}};
float P_pitch[2][2] = {{1, 0}, {0, 1}};
//



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

void Core0loop(void * parameter){
    for(;;){
            float gyro_rate_roll = gyroData.gyroX;
            float gyro_rate_pitch = gyroData.gyroY;
            float accel_angle_roll = accelData.accelX;
            float accel_angle_pitch = accelData.accelY;

            kalmanPredict(x_roll, P_roll, gyro_rate_roll);
            kalmanUpdate(x_roll, P_roll, accel_angle_roll);

            kalmanPredict(x_pitch, P_pitch, gyro_rate_pitch);
            kalmanUpdate(x_pitch, P_pitch, accel_angle_pitch);

            Serial.print("Roll: ");
            Serial.print(x_roll);
            Serial.print(" Pitch: ");
            Serial.println(x_pitch);

            output.pitch = x_pitch;
            output.roll = x_roll;
            delay(20);
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

    Serial.println("Data transmitted...");
}

// Reserved for kalman


void kalmanPredict(float &x_angle, float P[2][2], float gyro_rate)
{
    float F[2][2] = {{1, -dt}, {dt, 1}};
    float B[2] = {dt, 0};

    float x_new = F[0][0] * x_angle + F[0][1] * B[0] * gyro_rate;
    x_angle = x_new;

    float P_temp[2][2];
    P_temp[0][0] = F[0][0] * P[0][0] + F[0][1] * P[1][0];
    P_temp[0][1] = F[0][0] * P[0][1] + F[0][1] * P[1][1];
    P_temp[1][0] = F[1][0] * P[0][0] + F[1][1] * P[1][0];
    P_temp[1][1] = F[1][0] * P[0][1] + F[1][1] * P[1][1];

    P[0][0] = P_temp[0][0] + Q_angle;
    P[0][1] = P_temp[0][1];
    P[1][0] = P_temp[1][0];
    P[1][1] = P_temp[1][1] + Q_gyro;
}

void kalmanUpdate(float &x_angle, float P[2][2], float accel_angle)
{
    float H[2] = {1, 0};
    float y = accel_angle - (H[0] * x_angle);
    float S = P[0][0] + R_angle;

    float K[2];
    K[0] = P[0][0] / S;
    K[1] = P[1][0] / S;

    x_angle += K[0] * y;

    float P_temp[2][2];
    P_temp[0][0] = P[0][0] - K[0] * P[0][0];
    P_temp[0][1] = P[0][1] - K[0] * P[0][1];
    P_temp[1][0] = P[1][0] - K[1] * P[0][0];
    P_temp[1][1] = P[1][1] - K[1] * P[0][1];

    P[0][0] = P_temp[0][0];
    P[0][1] = P_temp[0][1];
    P[1][0] = P_temp[1][0];
    P[1][1] = P_temp[1][1];
}

//