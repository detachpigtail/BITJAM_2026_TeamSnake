namespace MotionCore2D.Input
{
    /// <summary>
    /// Abstraction over the source of an actor's movement intent. Decouples the motion system
    /// from any concrete input device or scheme, allowing player input, AI, replay, or networked
    /// sources to drive the same actor.
    /// </summary>
    public interface IMotionInput
    {
        /// <summary>
        /// Horizontal movement intent in the range -1 (full left) to 1 (full right).
        /// </summary>
        float Horizontal { get; }

        /// <summary>
        /// Whether the jump control is currently held down. Used by features such as variable
        /// jump height.
        /// </summary>
        bool JumpHeld { get; }

        /// <summary>
        /// Whether the crouch control is currently held down. Consumed by the optional crouch
        /// feature to lower the actor's stance; an actor with no crouch component simply ignores it.
        /// </summary>
        bool CrouchHeld { get; }

        /// <summary>
        /// Returns whether a jump press has occurred since this method was last called, and clears
        /// the pending press. Edge-based so a press registered between fixed steps is not missed.
        /// </summary>
        /// <returns><c>true</c> if a jump was pressed since the previous call; otherwise <c>false</c>.</returns>
        bool ConsumeJumpPressed();
    }
}
