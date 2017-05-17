bool val = false;
void setup() {
  // put your setup code here, to run once:
  pinMode(2,INPUT);
  Serial.begin(9600);
}

void loop() {
  // put your main code here, to run repeatedly:
  bool newVal = !digitalRead(2);
  if(val != newVal)
  {
    val = newVal;
    Serial.print("Val ");
    Serial.println(val);
  }
}
