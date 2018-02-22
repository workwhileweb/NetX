using System;

namespace Leaf.Net.Services.Cloudflare
{
    /// <summary>
    /// The exception that is thrown if Cloudflare clearance failed after the declared number of attempts.
    /// </summary>
    [Serializable]
    public class CloudflareException : Exception
    {
        public CloudflareException(int attempts) : this(attempts, $"Clearance failed after {attempts} attempt(s).") { }

        public CloudflareException(int attempts, string message) : base(message)
        {
            Attempts = attempts;
        }

        public CloudflareException(int attempts, string message, Exception inner) : base(message, inner)
        {
            Attempts = attempts;
        }

        /// <summary>
        /// Returns the number of failed clearance attempts.
        /// </summary>
        public int Attempts { get; }
    }
}