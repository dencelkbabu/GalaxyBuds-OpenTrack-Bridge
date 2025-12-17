namespace BudsHeadTrackingBridge.GalaxyBudsClient;

/// <summary>
/// Spatial audio control commands
/// Extracted from GalaxyBudsClient.Model.Constants
/// </summary>
public enum SpatialAudioControl
{
    Attach = 0,
    Detach = 1,
    AttachSuccess = 2,
    DetachSuccess = 3,
    KeepAlive = 4,
    WearOnOff = 5,
    QuerySensorSupported = 6,
    SpatialBufOn = 7,
    SpatialBufOff = 8,
    QueryGyroBiasExistence = 9,
    ManualGyrocalStart = 10,
    ManualGyrocalCancel = 11,
    ManualGyrocalQueryReady = 12,
    ResetGyroInUseBias = 13
}

/// <summary>
/// Spatial audio data event types
/// Extracted from GalaxyBudsClient.Model.Constants
/// </summary>
public enum SpatialAudioData
{
    Unknown,
    BudGrv = 32,
    WearOn = 33,
    WearOff = 34,
    BudGyrocal = 35,
    BudSensorStuck = 36,
    SensorSupported = 37,
    GyroBiasExistence = 38,
    ManualGyrocalReady = 39,
    ManualGyrocalNotReady = 40,
    BudGyrocalFail = 41
}

/// <summary>
/// Message IDs for SPP protocol
/// Extracted from GalaxyBudsClient.Message.SppMessageEnums
/// </summary>
public enum MsgIds
{
    SET_SPATIAL_AUDIO = 124,
    SPATIAL_AUDIO_CONTROL = 195,
    SPATIAL_AUDIO_DATA = 194
}
