#include <LGPS.h>
#include <LGSM.h>
#include <LBattery.h>
#include <LGPRS.h>
#include <LGPRSClient.h>
#include "I2Cdev.h"
#include "MPU6050.h"
#include "Wire.h"
#include <LBT.h>
#include <LBTServer.h>

#define SITE_URL "index.hu"
#define LOCATION_ARR_SIZE 24
#define RECEIVED_FROM_NUM_ARR_SIZE 20
#define ACCEL_TRESHOLD 50
#define TAMPER_ALERT_SIZE 35

MPU6050 accelgyro;
LGPRSClient client;
gpsSentenceInfoStruct info;
char gpsBuffer[256];
char batteryBuffer[256];
char* phoneNumber = "+441173257381";
char* smstext = "";
int16_t ax, ay, az;
float accelx, accely/*,accelz*/, axprev, ayprev/*,azprev*/, diffx, diffy/*,diffz*/;
unsigned long previousmillis = 0;
long interval = 1000;
bool isInitialLocationSet = false;
bool isAlertMode = true;
bool isOverrideAlertMode = false;
bool theftmode = false;

double latitude = 0.00;
double longitude = 0.00;
float altitude = 0.00;
float dop = 100.00; //dilution of precision
float geoid = 0.00;
float k_speed = 0.00, m_speed = 0.00; //speed in knots and speed in m/s
float track_angle = 0.00;
int fix = 0;
int hour = 0, minute = 0, second = 0;
int sat_num = 0; //number of visible satellites
int day = 0, month = 0, year = 0;
String time_format = "00:00:00", date_format = "00:00:0000";
String lat_format = "0.00000", lon_format = "0.00000";
int pause = 3000; //time in milliseconds between two logs
char locationDataCopy[LOCATION_ARR_SIZE];
char smsReceivedFromNumberBuffer[RECEIVED_FROM_NUM_ARR_SIZE];

void setup()
{
  Wire.begin();   //begin I2c
  Serial.begin(115200);

  // initialize accelerometer
  Serial.println("Initializing Accelerometer...");
  accelgyro.initialize();

  // verify connection
  Serial.println("Testing Accelerometer...");
  Serial.println(accelgyro.testConnection() ? "Accelerometer connection successful" : "Accelerometer connection failed");

  Serial.println("Starting SMS service");
  while (!LSMS.ready()) // Wait for the sim to initialize
  {
    delay(1000); // Wait for a second and then try again
  }
  Serial.println("Successfully started SMS service");

  while (!LGPRS.attachGPRS())
  {
    Serial.println("wait for SIM card ready");
    delay(1000);
  }
  Serial.println("GRPS attached");
  LGPS.powerOn();
  Serial.println("LGPS Power on, and waiting ...");

  if (!LBTServer.begin((uint8_t*)"BikeTracker"))
  {
    Serial.println("Failed to initialise bluetooth");
  }
  else
  {
    Serial.println("Bluetooth succesfully initialized");
  }

  //delay(3000);
}

void loop()
{

  if (LBTServer.connected())
  {
    isAlertMode = false;
  }
  else
  {
    isAlertMode = true;
    // Wait 5 secs and retry forever
    LBTServer.accept(5);
  }


  // put your main code here, to run repeatedly:
  if (!isInitialLocationSet)
  {
    getGPSData(0);
    char *data = locationDataCopy;
    SendSMS(phoneNumber, data);

    isInitialLocationSet = true;
  }

  if (isAlertMode) {
    getGPSData(1);
    char *data = locationDataCopy;
    SendSMS(phoneNumber, data);
  }

  //checkForAvailableSms();
  delay(1000);

  //get_accel();

  //DoGETRequest();
}

void checkForAvailableSms()
{
  if (LSMS.available())
  {
    Serial.println("New SMS messages are available");
    LSMS.remoteNumber(smsReceivedFromNumberBuffer, RECEIVED_FROM_NUM_ARR_SIZE);
    Serial.print("Number: ");
    Serial.println(smsReceivedFromNumberBuffer);

    Serial.print("Content: "); // display Content part

    int c;
    while (true)
    {
      c = LSMS.read(); // message content (one byte at a time)
      if (c < 0)
        break; // enf of message content
    }
    Serial.println();

    LSMS.flush();
  }
  else
  {
    Serial.println("No SMS found.");
  }
}

void get_accel()
{
  unsigned long currentmillis = millis();
  accelgyro.getAcceleration(&ax, &ay, &az);
  accelx = ax / 100;
  accely = ay / 100;
  //accelz = az / 100;
  diffx = abs(((axprev - accelx) / axprev) * 100);
  diffy = abs(((ayprev - accely) / ayprev) * 100);
  //diffz = abs(((azprev - accelz) / azprev) * 100);
  //Serial.print("X AXIS DIFFERENCE : \t"); Serial.print(diffx);Serial.print("%\n");
  //Serial.print("y AXIS DIFFERENCE : \t"); Serial.print(diffy);Serial.print("%\n");
  //Serial.print("z AXIS DIFFERENCE : \t"); Serial.print(diffz);Serial.print("%\n");
  axprev = accelx;
  ayprev = accely;
  //azprev = accelz;
  //Serial.print("accelerometer" "\t");
  //Serial.print(accelx); Serial.print("\t");
  //Serial.print(accely); Serial.print("\t");
  //Serial.print(accelz); Serial.print("\n");
  if (diffx >= ACCEL_TRESHOLD or diffy >= ACCEL_TRESHOLD)
  {
    if(currentMillis - previousMillis > interval) 
    {
      isAlertMode = true;
      smstext = "2;YOUR BIKE IS BEING TAMPERED WITH!"
      smstext.toCharArray(smstext, TAMPER_ALERT_SIZE);
      SendSMS(phoneNumber, smstext);
    }
  } else isAlertMode = false;
}

void getGPSData(int command)
{
  int satellitePrecision = 0;
  satellitePrecision = getData(&info);

  if (satellitePrecision >= 0)
  {
    String str = "";
    str += command;
    str += ";";
    //    str += date_format;
    //    str += ",";
    //    str += time_format;
    //    str += ",";
    str += lat_format;
    str += ";";
    str += lon_format;
    str += ";";
    str += satellitePrecision;
    //    str += altitude;
    //    str += ",";
    //    str += dop;
    //    str += ",";
    //    str += geoid;
    //    str += ",";
    //    str += track_angle;
    //    str += ",";
    //    str += m_speed;
    //    str += ",";
    //    str += k_speed;
    //    str += ",";
    //    str += fix;
    //    str += ",";
    //    str += sat_num;
    Serial.println(str);

    str.toCharArray(locationDataCopy, LOCATION_ARR_SIZE);
  }
}

void SendSMS(char *number, char *message)
{
  LSMS.beginSMS(number);
  LSMS.print(message); // Prepare message variable to be sent by LSMS
  Serial.print("sending sms to: ");
  Serial.println(number);
  Serial.print("Content: ");
  Serial.println(message);

  if (LSMS.endSMS()) // If so, send the SMS
  {
    Serial.println("SMS sent"); // Print "SMS sent" in serial port if sending is successful
  }
  else
  {
    Serial.println("SMS is not sent"); // Else print "SMS is not sent"
  }
}

void DisplayBatteryLevel()
{
  sprintf(batteryBuffer, "battery level = %d", LBattery.level());
  Serial.println(batteryBuffer);
  sprintf(batteryBuffer, "is charging = %d", LBattery.isCharging());
  Serial.println(batteryBuffer);
  delay(1000);
}

void DoGETRequest()
{ Serial.print("Connecting to : " SITE_URL "...");
  if (!client.connect(SITE_URL, 80))
  {
    Serial.println("FAIL!");
    return;
  }
  Serial.println("done");
  Serial.print("Sending GET request...");
  client.println("GET / HTTP/1.1");
  client.println("Host: " SITE_URL ":80");
  client.println();
  Serial.println("done");

  int v;
  while (client.available())
  {
    v = client.read();
    if (v < 0)
      break;
    Serial.write(v);
  }

  delay(500);

  if (!client.available() && !client.connected())
  {
    Serial.println();
    Serial.println("disconnecting.");
    client.stop();
  }
}

float convert(String str, boolean dir)
{
  double mm, dd;
  int point = str.indexOf('.');
  dd = str.substring(0, (point - 2)).toFloat();
  mm = str.substring(point - 2).toFloat() / 60.00;
  return (dir ? -1 : 1) * (dd + mm);
}

int getData(gpsSentenceInfoStruct* info)
{
  Serial.println("Collecting GPS data.");
  LGPS.getData(info);
  Serial.println((char*)info->GPGGA);
  if (info->GPGGA[0] == '$')
  {
    Serial.print("Parsing GGA data....");
    String str = (char*)(info->GPGGA);
    str = str.substring(str.indexOf(',') + 1);
    hour = str.substring(0, 2).toInt();
    minute = str.substring(2, 4).toInt();
    second = str.substring(4, 6).toInt();
    time_format = "";
    time_format += hour;
    time_format += ":";
    time_format += minute;
    time_format += ":";
    time_format += second;
    str = str.substring(str.indexOf(',') + 1);
    latitude = convert(str.substring(0, str.indexOf(',')), str.charAt(str.indexOf(',') + 1) == 'S');
    int val = latitude * 1000000;
    String s = String(val);
    lat_format = s.substring(0, (abs(latitude) < 100) ? 2 : 3);
    lat_format += '.';
    lat_format += s.substring((abs(latitude) < 100) ? 2 : 3);
    str = str.substring(str.indexOf(',') + 3);
    longitude = convert(str.substring(0, str.indexOf(',')), str.charAt(str.indexOf(',') + 1) == 'W');
    val = longitude * 1000000;
    s = String(val);
    lon_format = s.substring(0, (abs(longitude) < 100) ? 2 : 3);
    lon_format += '.';
    lon_format += s.substring((abs(longitude) < 100) ? 2 : 3);

    str = str.substring(str.indexOf(',') + 3);
    fix = str.charAt(0) - 48;
    str = str.substring(2);
    sat_num = str.substring(0, 2).toInt();
    str = str.substring(3);
    dop = str.substring(0, str.indexOf(',')).toFloat();
    str = str.substring(str.indexOf(',') + 1);
    altitude = str.substring(0, str.indexOf(',')).toFloat();
    str = str.substring(str.indexOf(',') + 3);
    geoid = str.substring(0, str.indexOf(',')).toFloat();
    Serial.println("done.");

    if (info->GPRMC[0] == '$')
    {
      Serial.print("Parsing RMC data....");
      str = (char*)(info->GPRMC);
      int comma = 0;
      for (int i = 0; i < 60; ++i)
      {
        if (info->GPRMC[i] == ',')
        {
          comma++;
          if (comma == 7)
          {
            comma = i + 1;
            break;
          }
        }
      }

      str = str.substring(comma);
      k_speed = str.substring(0, str.indexOf(',')).toFloat();
      m_speed = k_speed * 0.514;
      str = str.substring(str.indexOf(',') + 1);
      track_angle = str.substring(0, str.indexOf(',')).toFloat();
      str = str.substring(str.indexOf(',') + 1);
      day = str.substring(0, 2).toInt();
      month = str.substring(2, 4).toInt();
      year = str.substring(4, 6).toInt();
      date_format = "20";
      date_format += year;
      date_format += "-";
      date_format += month;
      date_format += "-";
      date_format += day;
      Serial.println("done.");
      return sat_num;
    }
  }
  else
  {
    Serial.println("No GGA data");
  }
  return 0;
}
