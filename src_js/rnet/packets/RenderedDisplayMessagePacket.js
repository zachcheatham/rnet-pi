const RNetPacket = require("./RNetPacket");
const SmartBuffer = require("smart-buffer").SmartBuffer;

class RenderedDisplayMessagePacket extends RNetPacket {
    constructor() {
        super();
        this.messageType = 0x06;
    }

    getLowValue() {
        return this.valueLow;
    }

    getHighValue() {
        return this.valueHigh;
    }

    getShortValue() {
        return (this.valueHigh << 4) | this.valueLow;
    }

    getFlashTime() {
        return this.flashTime;
    }

    getRenderType() {
        return this.renderType;
    }
}

RenderedDisplayMessagePacket.fromPacket = function(rNetPacket) {
    if (rNetPacket instanceof RNetPacket) {
        const rdmPacket = new RenderedDisplayMessagePacket();
        rNetPacket.copyToPacket(rdmPacket);

        const buffer = new SmartBuffer(rNetPacket.messageBody);

        rdmPacket.valueLow = buffer.readUInt8();
        rdmPacket.valueHigh = buffer.readUInt8();
        rdmPacket.flashTime = buffer.readUInt16LE();
        rdmPacket.renderType = buffer.readUInt8();

        return rdmPacket;
    }
    else {
        throw new TypeError("Cannot create RenderedDisplayMessagePacket with anything other than RNetPacket");
    }
}

RenderedDisplayMessagePacket.TYPE_DIRECTORY = 0;
RenderedDisplayMessagePacket.TYPE_UINT8_DEC_NUM = 1;
RenderedDisplayMessagePacket.TYPE_UINT16_DEC_NUM = 2;
RenderedDisplayMessagePacket.TYPE_STRING = 3;
RenderedDisplayMessagePacket.TYPE_SOURCE_NUM = 4;
RenderedDisplayMessagePacket.TYPE_SOURCE_NAME = 5;
RenderedDisplayMessagePacket.TYPE_DEVICE_TYPE_NAME = 6;
RenderedDisplayMessagePacket.TYPE_BOOL_SELECT = 7;
RenderedDisplayMessagePacket.TYPE_NUMERIC_PREFIX = 8;
RenderedDisplayMessagePacket.TYPE_KEY_NAME = 9;
RenderedDisplayMessagePacket.TYPE_ON_OFF = 10;
RenderedDisplayMessagePacket.TYPE_CHILD_NAME = 11;
RenderedDisplayMessagePacket.TYPE_YES_NO = 12;
RenderedDisplayMessagePacket.TYPE_UINT8_DEC_NUM_PLUS_ONE = 13;
RenderedDisplayMessagePacket.TYPE_UINT16_DEC_NUM_FIXED_WIDTH = 14;
RenderedDisplayMessagePacket.TYPE_UINT8_HEX_ARRAY = 15;
RenderedDisplayMessagePacket.TYPE_VOLUME = 16;
RenderedDisplayMessagePacket.TYPE_BASS = 17;
RenderedDisplayMessagePacket.TYPE_TREBLE = 18;
RenderedDisplayMessagePacket.TYPE_BALANCE = 19;
RenderedDisplayMessagePacket.TYPE_BACKGROUND_COLOR = 20;
RenderedDisplayMessagePacket.TYPE_DEFAULT_KEY_NAME = 21;
RenderedDisplayMessagePacket.TYPE_DEFAULT_KEY_TYPE = 22;
RenderedDisplayMessagePacket.TYPE_UINT8_DEC_NUM_FIXED_WIDTH = 23;
RenderedDisplayMessagePacket.TYPE_ALL_STRINGS = 24;
RenderedDisplayMessagePacket.TYPE_CONTROLLER_ID = 25;
RenderedDisplayMessagePacket.TYPE_NUMERIC_SCROLL_NAMES = 26;
RenderedDisplayMessagePacket.TYPE_DELAY_TIME = 27;
RenderedDisplayMessagePacket.TYPE_CAV_PARAM_NAME = 28;
RenderedDisplayMessagePacket.TYPE_MACRO_NAME = 29;
RenderedDisplayMessagePacket.TYPE_KEYCODE_NAME = 30;
RenderedDisplayMessagePacket.TYPE_SOURCE = 31;
RenderedDisplayMessagePacket.TYPE_SAVE_TO_ZONES = 32;
RenderedDisplayMessagePacket.TYPE_ENABLE_DISABLE = 33;
RenderedDisplayMessagePacket.TYPE_DEVICE_CODE = 34;
RenderedDisplayMessagePacket.TYPE_DISK_NUMERIC_SCROLL = 35;
RenderedDisplayMessagePacket.TYPE_CHANNEL_NUMERIC_SCROLL = 36;
RenderedDisplayMessagePacket.TYPE_PRESET_NUMERIC_SCROLL = 37;
RenderedDisplayMessagePacket.TYPE_VOLUME_TRIM = 38;
RenderedDisplayMessagePacket.TYPE_PARTY_MODE = 39;
RenderedDisplayMessagePacket.TYPE_SECONDS = 40;
RenderedDisplayMessagePacket.TYPE_CHAR_TEST = 41;
RenderedDisplayMessagePacket.TYPE_BLOCK_TEST = 42;
RenderedDisplayMessagePacket.TYPE_ROW_TEST = 43;
RenderedDisplayMessagePacket.TYPE_SENSE_DELAY = 44;
RenderedDisplayMessagePacket.TYPE_LEARNED_SOURCE_NUM = 45;
RenderedDisplayMessagePacket.TYPE_LEARN_DELETE = 46;
RenderedDisplayMessagePacket.TYPE_MACRO_STEP_NUM = 47;
RenderedDisplayMessagePacket.TYPE_PORT_ID = 48;
RenderedDisplayMessagePacket.TYPE_KEYPAD_ID = 49;
RenderedDisplayMessagePacket.TYPE_RECEIVER_KEY_NAME = 50;
RenderedDisplayMessagePacket.TYPE_RECEIVER_DEVICE_TYPE_NAME = 51;

module.exports = RenderedDisplayMessagePacket;
