using System;

namespace Leaf.Net.Services.StormWall
{
    /// <inheritdoc />
    /// <summary>
    /// The exception that is thrown if StormWall clearance failed.
    /// </summary>
    [Serializable]
    public class StormWallException : Exception
    {
        public StormWallException() { }

        public StormWallException(string message) : base(message) {}

        public StormWallException(string message, Exception inner) : base(message, inner) {}
    }
}