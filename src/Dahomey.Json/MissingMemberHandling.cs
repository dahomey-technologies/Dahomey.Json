namespace Dahomey.Json
{
    /// <summary>
    /// Specifies missing member handling options for the JsonSerializer.
    /// </summary>
    public enum MissingMemberHandling
    {
        /// <summary>
        /// Ignore a missing member and do not attempt to deserialize it.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Throw a JsonException when a missing member is encountered during deserialization.
        /// </summary>
        Error = 1
    }
}
