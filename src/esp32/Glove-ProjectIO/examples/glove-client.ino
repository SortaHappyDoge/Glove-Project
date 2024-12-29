#include <Arduino.h>

const int onboard_button = 0; // GPIO0 BOOT button on esp32 that will be used for confirmation/skipping certain actions *will remove in the future*

#include <I2Cdev.h>
#include <MPU6050_6Axis_MotionApps20.h>
MPU6050 mpu(0x69);
bool DMPReady = false; // Set true if DMP init was successful

uint8_t devStatus;      // Return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // Expected DMP packet size (default is 42 bytes)
uint8_t FIFOBuffer[64]; // FIFO storage buffer

Quaternion q;        // [w, x, y, z]         Quaternion container
VectorInt16 aa;      // [x, y, z]            Accel sensor measurements
VectorInt16 gy;      // [x, y, z]            Gyro sensor measurements
float aaReal[3];     // [x, y, z]            Gravity-free accel sensor measurements
VectorInt16 aaWorld; // [x, y, z]            World-frame accel sensor measurements
VectorFloat gravity; // [x, y, z]            Gravity vector
float euler[3];      // [psi, theta, phi]    Euler angle container
float ypr[3];        // [yaw, pitch, roll]   Yaw/Pitch/Roll container and gravity vector

#include <WiFi.h>
#include <WiFiUdp.h>

const char *default_ssid = "Redmii"; // Default WiFi SSID that will be used if input process is skipped
const char *default_password = "cokgizli";    // Default WiFi password that will be used if input process is skipped

const char *default_connection_ip = "192.168.147.43"; // Default "Data Reciever Server" IP that will be used if input process is skipped
const u_int16_t server_port = 8000;                 //"Data Reciever Server" port
WiFiUDP UDP_client;                                 // Defined WiFiUDP class
float mpu_data[10];                             // Contains the array of floats that are sent {accelX, accelY, accelZ, roll, pitch, yaw, {Quaternion}}
float message_data[11];


char *ssid;
char *password;
char *connection_ip;

const int mpu_pins[] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
const float mpu_offsets[] = 
{   -5416, -426, 1420, 84, -30, 47, //mpu_0
    -2356, -392, 874, 81, 10, -23,  //mpu_1
    22, -2741, 840, 100, 52, 35,    //mpu_2
    -1870, 586, 1142, 23, -77, -5,  //mpu_3
    1344, -1490, 1358, 45, -58, -80,//mpu_4
    -3768, -1911, 2008, 79, -2, 1,  //mpu_5
    -4036, -1502, 1030, 25, -8, 21, //mpu_6
    -5196, -1672, 1096, 126, -80, 3,//mpu_7
    4946, 1122, 1178, 68, 0, -29,   //mpu_8
    -1154, 308, 1118, 87, 52, -25,   //mpu_9
    -3472, -990, 1304, 97, 1, -34,  //mpu_10
    -444, -3031, 778, 38, 16, 4     //mpu_11
};

/**
 * Can be skipped by pressing the BOOT button on the esp32,
 * Takes user input from the serial monitor of the following:
 * @param char* *id - WiFi SSID with the "Data Reciever Server" running locally
 * @param char* *pass - The password to the given network
 * @param char* *ip - The local IP address that the "Data Reciever Server" is running on
 * @warning Won't be useful if the esp32 is not connected to serial
 */
void take_user_inputs(char **id, char **pass, char **ip)
{
    // ssid = (char*)default_ssid; password = (char*)default_password; connection_ip = (char*)default_connection_ip;
    *id = (char *)malloc(strlen(default_ssid) + 1);
    *pass = (char *)malloc(strlen(default_password) + 1);
    *ip = (char *)malloc(strlen(default_connection_ip) + 1);
    strcpy(*id, default_ssid);
    strcpy(*pass, default_password);
    strcpy(*ip, default_connection_ip);

    bool input_process = true;
    Serial.println("SSID, password, ip: ");

    int i = 3;
    while (input_process)
    {
        if (!digitalRead(onboard_button))
        {
            input_process = false;
            Serial.println("Aborted input process");
            break;
        };
        if (Serial.available() != 0)
        {
            String buff = Serial.readStringUntil('\n');
            buff.trim();
            switch (i)
            {
            case 3:
                free(*id);
                *id = (char *)malloc(sizeof(buff));
                buff.toCharArray(*id, sizeof(buff));
                break;
            case 2:
                free(*pass);
                *pass = (char *)malloc(sizeof(buff));
                buff.toCharArray(*pass, sizeof(buff));
                break;
            case 1:
                free(*ip);
                *pass = (char *)malloc(sizeof(buff));
                buff.toCharArray(*ip, sizeof(buff));
                Serial.println("All inputs are saved");
                input_process = false;
                break;
            }
            --i;
        }
    }
}

/**
 * Initiates connection to the local network (WiFi)
 * @param char* id - WiFi SSID with the "Data Reciever Server" running locally
 * @param char* pass - The password to the given network
 * @warning No timeout coded in, if either the SSID or the password wrong it will loop to infinity
 */
void initiate_connection(char *id, char *pass)
{
    WiFi.begin(id, pass);

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
        Serial.println("...");
    }

    Serial.print("WiFi connected with IP:");
    Serial.println(WiFi.localIP());
}

/**
 *
 * @param char* ip - The ip of the server you want to send the "message" to
 * @param {unsigned char*} message - The "message" you want to send as a byte array
 */
void send_data_to_server(char *ip, byte message[], int length, uint8_t message_type)
{
    if (WiFi.status() != WL_CONNECTED)
    {
        initiate_connection(ssid, password);
    }

    UDP_client.beginPacket(ip, server_port);
    UDP_client.write(message_type);
    UDP_client.write(message, length);
    UDP_client.endPacket();
}

void mpu_dmp_recieve(void *)
{
    if (!DMPReady)
        return; // Stop the program if DMP programming fails.

    /* Read a packet from FIFO */
    if (mpu.dmpGetCurrentFIFOPacket(FIFOBuffer))
    { // Get the Latest packet
        mpu.dmpGetQuaternion(&q, FIFOBuffer);
        mpu.dmpGetAccel(&aa, FIFOBuffer);
        mpu.dmpGetGravity(&gravity, &q);
        // mpu.dmpGetLinearAccel(aaReal, , &gravity);
        mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
        mpu_data[0] = aa.x;
        mpu_data[1] = aa.y;
        mpu_data[2] = aa.z;
        mpu_data[3] = ypr[2];
        mpu_data[4] = ypr[1];
        mpu_data[5] = ypr[0];
        mpu_data[6] = q.w;
        mpu_data[7] = q.x;
        mpu_data[8] = q.y;
        mpu_data[9] = q.z;
    }
}

void initialize_mpus(){
    /* Repeat MPU initilization for every single MPU in the list */
    for (unsigned int i = sizeof(mpu_pins)/sizeof(mpu_pins[0]); i > 0; i--)
    {
        pinMode(mpu_pins[i-1], OUTPUT);
        digitalWrite(mpu_pins[i-1], HIGH);

        Serial.println("Initializing MPU...");
        mpu.initialize();

        Serial.println("Testing MPU6050 connection...");
        if (mpu.testConnection() == false)
        {
            Serial.println("MPU6050 connection failed");
            Serial.println(mpu_pins[i-1]);
            while (true)
                ;
        }
        else
        {
            Serial.println("MPU6050 connection successful");
        }

        /* Initializate and configure the DMP*/
        Serial.println(F("Initializing DMP..."));
        devStatus = mpu.dmpInitialize();

        /* Supply your gyro offsets here, scaled for min sensitivity */
        mpu.setXGyroOffset(mpu_offsets[3+(i-1)*6]);
        mpu.setYGyroOffset(mpu_offsets[4+(i-1)*6]);
        mpu.setZGyroOffset(mpu_offsets[5+(i-1)*6]);
        mpu.setXAccelOffset(mpu_offsets[0+(i-1)*6]);
        mpu.setYAccelOffset(mpu_offsets[1+(i-1)*6]);
        mpu.setZAccelOffset(mpu_offsets[2+(i-1)*6]);

        /* Making sure it worked (returns 0 if so) */
        if (devStatus == 0)
        {
            /*mpu.CalibrateAccel(6); // Calibration Time: generate offsets and calibrate our MPU6050
            mpu.CalibrateGyro(6);
            Serial.println("These are the Active offsets: ");
            mpu.PrintActiveOffsets();*/
            Serial.println(F("Enabling DMP...")); // Turning ON DMP
            mpu.setDMPEnabled(true);

            /* Set the DMP Ready flag so the main loop() function knows it is okay to use it */
            DMPReady = true;
            packetSize = mpu.dmpGetFIFOPacketSize(); // Get expected DMP packet size for later comparison
        }
        else
        {
            Serial.print(F("DMP Initialization failed (code ")); // Print the error code
            Serial.print(devStatus);
            Serial.println(F(")"));
            Serial.println(mpu_pins[i-1]);
            while (true)
            {
                ;
            }
            // 1 = initial memory load failed
            // 2 = DMP configuration updates failed
        }
        digitalWrite(mpu_pins[i-1], LOW);
    }
}

float* receive_mpus(){
    //
    size_t size = (sizeof(mpu_pins)/sizeof(mpu_pins[0]))*(sizeof(message_data)/sizeof(message_data[0]));
    float* data = (float*)malloc(size * sizeof(float));
    for(unsigned int i = sizeof(mpu_pins)/sizeof(mpu_pins[0]); i > 0; i--){
        digitalWrite(mpu_pins[i-1], HIGH);
        mpu_dmp_recieve(&mpu_data);
        digitalWrite(mpu_pins[i-1], LOW);
        
        memcpy(data + (i - 1)*((size)/(sizeof(mpu_pins)/sizeof(mpu_pins[0]))), mpu_data, sizeof(mpu_data));
        data[i*11-1] = mpu_pins[i-1];
    }

    return data;
}


void setup()
{
    pinMode(onboard_button, INPUT);

    Wire.begin();
    Wire.setClock(400000);
    Serial.begin(115200);
    while (!Serial)
        ;

    take_user_inputs(&ssid, &password, &connection_ip);

    initiate_connection(ssid, password);
    initialize_mpus();
}

void loop()
{
    //mpu_dmp_recieve(&mpu_data);
    float* message_ptr = receive_mpus();
    float message[(sizeof(mpu_pins)/sizeof(mpu_pins[0]))*(sizeof(message_data)/sizeof(message_data[0]))];
    //memcpy(message, message_ptr, sizeof(message_ptr));

    for(int i = sizeof(message)/sizeof(message[0]); i>0; i--){
        message[i-1] = message_ptr[i-1];
    }
    //Serial.print(message[14]); Serial.print("\t"); Serial.print(message[15]); Serial.print("\t"); Serial.println(message[16]);// Serial.print("\t"); Serial.print(message[20]); Serial.println("\t");

    byte buffer[sizeof(message)];
    memcpy(buffer, message, sizeof(message));
    send_data_to_server(connection_ip, buffer, sizeof(buffer), 0x00);
    free(message_ptr);
    delay(10);
}