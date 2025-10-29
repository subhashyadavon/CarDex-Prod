namespace CarDexBackend.Shared.Validator
{
    /// <summary>
    /// Extension methods for registering validators in the application pipeline.
    /// </summary>
    public static class ValidatorExtensions
    {
        /// <summary>
        /// Adds the token validator middleware to the application pipeline.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseTokenValidator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidator>();
        }
    }
}

