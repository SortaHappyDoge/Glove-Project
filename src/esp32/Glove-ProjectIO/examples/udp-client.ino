#include <Arduino.h>
#include <WiFi.h>
#include <WiFiUdp.h>

const char* ssid = "TurkTelekom_Z7FMA";
const char* password = "3f17Gm9s61";

const char* connection_ip = "192.168.1.37";
int port = 8000;
WiFiUDP client;
size_t message_size = 1024;

struct Msg{
    float m;
    float n;
}; Msg message;


void setup(){
    message.m = 1; message.n = -1;
    connect_to_network();
}

void loop(){
    run_client();
    delay(20);
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

void run_client(){
  client.beginPacket(connection_ip, port);
  client.write((uint8_t*)&message, sizeof(message));
  client.endPacket();
  
  Serial.println("Data transmitted...");
}
