#include <LGPS.h>
#include <LGSM.h>
#include <LBattery.h>
#include <LGPRS.h>
#include <LGPRSClient.h>

gpsSentenceInfoStruct info;
char gpsBuffer[256];
char batteryBuffer[256];
const char *phoneNumber = "+447941573861";

static unsigned char getComma(unsigned char num, const char *str)
{
  unsigned char i, j = 0;
  int len = strlen(str);
  for (i = 0; i < len; i++)
  {
    if (str[i] == ',')
      j++;
    if (j == num)
      return i + 1;
  }
  return 0;
}

static double getDoubleNumber(const char *s)
{
  char buf[10];
  unsigned char i;
  double rev;

  i = getComma(1, s);
  i = i - 1;
  strncpy(buf, s, i);
  buf[i] = 0;
  rev = atof(buf);
  return rev;
}

static double getIntNumber(const char *s)
{
  char buf[10];
  unsigned char i;
  double rev;

  i = getComma(1, s);
  i = i - 1;
  strncpy(buf, s, i);
  buf[i] = 0;
  rev = atoi(buf);
  return rev;
}

void parseGPGGA(const char *GPGGAstr)
{
  /* Refer to http://www.gpsinformation.org/dale/nmea.htm#GGA
   * Sample data: $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47
   * Where:
   *  GGA          Global Positioning System Fix Data
   *  123519       Fix taken at 12:35:19 UTC
   *  4807.038,N   Latitude 48 deg 07.038' N
   *  01131.000,E  Longitude 11 deg 31.000' E
   *  1            Fix quality: 0 = invalid
   *                            1 = GPS fix (SPS)
   *                            2 = DGPS fix
   *                            3 = PPS fix
   *                            4 = Real Time Kinematic
   *                            5 = Float RTK
   *                            6 = estimated (dead reckoning) (2.3 feature)
   *                            7 = Manual input mode
   *                            8 = Simulation mode
   *  08           Number of satellites being tracked
   *  0.9          Horizontal dilution of position
   *  545.4,M      Altitude, Meters, above mean sea level
   *  46.9,M       Height of geoid (mean sea level) above WGS84
   *                   ellipsoid
   *  (empty field) time in seconds since last DGPS update
   *  (empty field) DGPS station ID number
   *  *47          the checksum data, always begins with *
   */
  double latitude;
  double longitude;
  int tmp, hour, minute, second, num;
  if (GPGGAstr[0] == '$')
  {
    tmp = getComma(1, GPGGAstr);
    hour = (GPGGAstr[tmp + 0] - '0') * 10 + (GPGGAstr[tmp + 1] - '0');
    minute = (GPGGAstr[tmp + 2] - '0') * 10 + (GPGGAstr[tmp + 3] - '0');
    second = (GPGGAstr[tmp + 4] - '0') * 10 + (GPGGAstr[tmp + 5] - '0');

    sprintf(gpsBuffer, "UTC timer %2d-%2d-%2d", hour, minute, second);
    Serial.println(gpsBuffer);

    tmp = getComma(2, GPGGAstr);
    latitude = getDoubleNumber(&GPGGAstr[tmp]);
    tmp = getComma(4, GPGGAstr);
    longitude = getDoubleNumber(&GPGGAstr[tmp]);
    sprintf(gpsBuffer, "latitude = %10.4f, longitude = %10.4f", latitude, longitude);
    Serial.println(gpsBuffer);

    tmp = getComma(7, GPGGAstr);
    num = getIntNumber(&GPGGAstr[tmp]);
    sprintf(gpsBuffer, "satellites number = %d", num);
    Serial.println(gpsBuffer);
  }
  else
  {
    Serial.println("Not get data");
  }
}

void setup()
{
  Serial.begin(115200);

  Serial.println("Starting SMS service");
  while (!LSMS.ready()) // Wait for the sim to initialize
  {
    delay(1000); // Wait for a second and then try again
  }

  while (!LGPRS.attachGPRS())
  {
    Serial.println("wait for SIM card ready");
    delay(1000);
  }
  Serial.println("GRPS attached");
  LGPS.powerOn();
  Serial.println("LGPS Power on, and waiting ...");

  delay(3000);
}

void loop()
{
  // put your main code here, to run repeatedly:

  // getGPSData();
  delay(2000);
}

void getGPSData()
{
  Serial.println("Getting LGPS Data");
  LGPS.getData(&info);
  Serial.println((char *)info.GPGGA);
  parseGPGGA((const char *)info.GPGGA);
}

void SendSMS(char *number, char *message)
{
  LSMS.beginSMS(number);
  LSMS.print(message); // Prepare message variable to be sent by LSMS

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
  sprintf(buff, "battery level = %d", LBattery.level());
  Serial.println(buff);
  sprintf(buff, "is charging = %d", LBattery.isCharging());
  Serial.println(buff);
  delay(1000);
}

void DoGETRequest()
{
  Serial.print("Connecting to : " SITE_URL "...");
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

    // do nothing forevermore:
    for (;;)
      ;
  }
}
