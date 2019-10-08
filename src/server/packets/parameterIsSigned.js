module.exports = function(parameterID) {
    switch (parameterID) {
        case 0: // Bass
        case 1: // Treble
        case 3: // Balance
            return true;
        case 2: // Loudness
        case 4: // Turn on Volume
        case 5: // Background Color
        case 6: // Do Not Disturb
        case 7: // Party Mode
        case 8: // Front A/V Enable
            return false;
        default:
            throw new Error("Unexpected Parameter ID");
    }
}
