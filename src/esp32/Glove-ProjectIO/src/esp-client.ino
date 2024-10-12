#include <Arduino.h>

const int onboard_button = 0; //GPIO0 BOOT button on esp32 that will be used for confirmation/skipping certain actions *will remove in the future*


#include <WiFi.h>
#include <WiFiUdp.h>

const char* default_ssid = "TurkTelekom_Z7FMA"; //Default WiFi SSID that will be used if input process is skipped
const char* default_password = "3f17Gm9s61";    //Default WiFi password that will be used if input process is skipped

const char* default_connection_ip = "192.168.1.48"; //Default "Data Reciever Server" IP that will be used if input process is skipped
const u_int16_t server_port = 8000; //"Data Reciever Server" port
WiFiUDP UDP_client;     //Defined WiFiUDP class



char* ssid;
char* password;
char* connection_ip;


/**
 * Can be skipped by pressing the BOOT button on the esp32,
 * Takes user input from the serial monitor of the following:
 * @param char* *id - WiFi SSID with the "Data Reciever Server" running locally
 * @param char* *pass - The password to the given network
 * @param char* *ip - The local IP address that the "Data Reciever Server" is running on
 * @warning Won't be useful if the esp32 is not connected to serial
*/
void take_user_inputs(char* *id, char* *pass, char* *ip){
    strcpy(ssid, default_ssid); strcpy(password, default_password); strcpy(connection_ip, default_connection_ip);

    bool input_process = true;
    Serial.println("SSID, password, ip: ");

    int i = 3;
    while (input_process){
        if(!digitalRead(onboard_button)){
                input_process = false;
                Serial.println("Aborted input process");
                break;
            };
        if(Serial.available() != 0){
            String buff = Serial.readString();
            buff.trim();
            switch (i){
            case 3:
                buff.toCharArray(*id, sizeof(buff));
                break;
            case 2:
                buff.toCharArray(*pass, sizeof(buff));
                break;
            case 1:
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
void initiate_connection(char* id, char* pass){
    WiFi.begin(id, pass);

    while (WiFi.status() != WL_CONNECTED){
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
void send_data_to_server(char* ip, char* message, uint8_t message_type){
    if(WiFi.status() != WL_CONNECTED){initiate_connection(ssid, password);}

    UDP_client.beginPacket(ip, server_port);
    UDP_client.write((uint8_t *)&message_type, sizeof(message_type));
    UDP_client.write((uint8_t *)&message, sizeof(message));
    UDP_client.endPacket();
}


void setup(){
    pinMode(onboard_button, INPUT);

    Serial.begin(115200); while (!Serial){;}
    
    take_user_inputs(&ssid, &password, &connection_ip);

    initiate_connection(ssid, password);
}

void loop(){
    float f[4] = {0.03, 0.12, 5.134, 326.32};

    char buffer[sizeof(f)];
    memcpy(buffer, &f, sizeof(f));

    send_data_to_server(connection_ip, buffer, 0x00);
}