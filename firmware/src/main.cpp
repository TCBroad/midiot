#include <Arduino.h>
#include <SoftwareSerial.h>
#include <MIDI.h>
#include "avdweb_Switch.h"

#define MIDI_OUT_PIN PB0
#define LED_PIN PB1
#define BUTTON_PIN PB2

SoftwareSerial serial(PB5, MIDI_OUT_PIN);
MIDI_CREATE_INSTANCE(SoftwareSerial, serial, MIDI);

Switch pushButton = Switch(BUTTON_PIN);

void flash() {
    digitalWrite(LED_PIN, HIGH);
    delay(200);
    digitalWrite(LED_PIN, LOW);
}

void buttonCallback(void*) {
    MIDI.sendProgramChange(0, 1);

    flash();
}

void setup() {
    pinMode(LED_PIN, OUTPUT);

    flash();

    pushButton.setPushedCallback(&buttonCallback);

    MIDI.begin(MIDI_CHANNEL_OMNI);
}

void loop() {
    pushButton.poll();
}