namespace CarDexBackend.Domain.Enums
{
    /// <summary>
    /// Represents the rarity grade or level of a trading card in the CarDex system.
    /// Higher grades indicate rarer and more valuable cards.
    /// The grade affects the card's market value and desirability in trades.
    /// </summary>
    public enum GradeEnum
    {
        /// <summary>
        /// Factory grade - the standard, most common grade for cards.
        /// This is the default grade for cards and has the lowest rarity.
        /// </summary>
        FACTORY,

        /// <summary>
        /// Limited Run grade - a higher rarity tier indicating the card was produced in limited quantities.
        /// More valuable than FACTORY grade cards.
        /// </summary>
        LIMITED_RUN,

        /// <summary>
        /// NISMO grade - the highest rarity tier, representing the most exclusive and valuable cards.
        /// Named after Nissan's performance division, this is the premium grade for ultra-rare cards.
        /// </summary>
        NISMO
    }
}
