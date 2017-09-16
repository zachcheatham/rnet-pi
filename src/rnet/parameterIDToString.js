module.exports = function(parameterID) {
    switch (parameterID) {
        case 0:
            return "Bass";
        case 1:
            return "Treble";
        case 3:
            return "Loudness";
        case 4:
            return "Balance";
        case 5:
            return "Turn on Volume";
        case 6:
            return "Background Color";
        case 7:
            return "Do Not Disturb"
        case 8:
            return "Party Mode";
        case 9:
            return "Front AV Enable";
        default:
            return "Unknown";
    }
}
