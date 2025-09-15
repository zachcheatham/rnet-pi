namespace RNetPi.Core.Utilities;

public static class ParameterUtils
{
    /// <summary>
    /// Determines if a parameter ID represents a signed value
    /// </summary>
    /// <param name="parameterID">The parameter ID to check</param>
    /// <returns>True if the parameter is signed, false if unsigned</returns>
    public static bool IsParameterSigned(byte parameterID)
    {
        return parameterID switch
        {
            0 => true,  // Bass
            1 => true,  // Treble
            3 => true,  // Balance
            2 => false, // Loudness
            4 => false, // Turn on Volume
            5 => false, // Background Color
            6 => false, // Do Not Disturb
            7 => false, // Party Mode
            8 => false, // Front A/V Enable
            _ => throw new ArgumentException($"Unexpected Parameter ID: {parameterID}", nameof(parameterID))
        };
    }
    
    /// <summary>
    /// Determines if a parameter ID represents a boolean value
    /// </summary>
    /// <param name="parameterID">The parameter ID to check</param>
    /// <returns>True if the parameter is a boolean, false otherwise</returns>
    public static bool IsParameterBoolean(byte parameterID)
    {
        return parameterID switch
        {
            2 => true,  // Loudness
            6 => true,  // Do Not Disturb
            8 => true,  // Front A/V Enable
            _ => false
        };
    }
}