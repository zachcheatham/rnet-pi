module.exports = function(parameterID) {
    switch (parameterID) {
        case 0:
            return "Bass";
        case 1:
            return "Treble";
        case 2:
            return "Loudness";
        case 3:
            return "Balance";
        case 4:
            return "Turn on Volume";
        case 5:
            return "Background Color";
        case 6:
            return "Do Not Disturb"
        case 7:
            return "Party Mode";
        case 8:
            return "Front AV Enable";
        default:
            return "Unknown";
    }
}
