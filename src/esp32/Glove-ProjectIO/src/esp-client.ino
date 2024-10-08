#include <Arduino.h>

const int onboard_button = 0; //GPIO0 BOOT button on esp32 that will be used for confirmation/skipping certain actions *will remove in the future*


#include <WiFi.h>
#include <WiFiUdp.h>

const String default_ssid = "TurkTelekom_Z7FMA"; //Default WiFi SSID that will be used if input process is skipped
const String default_password = "3f17Gm9s61";    //Default WiFi password that will be used if input process is skipped

const char* default_connection_ip = "192.168.1.48"; //Default "Data Reciever Server" IP that will be used if input process is skipped
int server_port = 8000; //"Data Reciever Server" port
WiFiUDP UDP_client;     //Defined WiFiUDP class



String ssid;
String password;
String connection_ip;

/**
 * Can be skipped by pressing the BOOT button on the esp32,
 * Takes user input from the serial monitor of the following:
 * @param String *id - WiFi SSID with the "Data Reciever Server" running locally
 * @param String *pass - The password to the given network
 * @param String *ip - The local IP address that the "Data Reciever Server" is running on
 * @warning Won't be useful if the esp32 is not connected to serial
*/
void take_user_inputs(String *id, String *pass, String *ip){
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
            switch (i){
            case 3:
                *id = Serial.readString();
                id->trim();
                break;
            case 2:
                *pass = Serial.readString();
                pass->trim();
                break;
            case 1:
                *ip = Serial.readString();
                ip->trim();
                Serial.println("All inputs are saved");
                input_process = false;
                break;
            }
            --i;
        }
    }
}

void setup(){
    pinMode(onboard_button, INPUT);

    Serial.begin(115200);
    
    
    
    take_user_inputs(&ssid, &password, &connection_ip);

    Serial.println(ssid);
    Serial.println(password);
    Serial.println(connection_ip);
}

void loop(){
    ;
}