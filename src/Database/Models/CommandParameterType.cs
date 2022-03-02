namespace Brighid.Commands.Service
{
    /// <summary>
    /// Represents a type of command parameter.
    /// </summary>
    public enum CommandParameterType
    {
        /// <summary>
        /// Parameter that is a sequence of any characters.
        /// </summary>
        String = 0,

        /// <summary>
        /// Parameter that is numeric.
        /// </summary>
        Number = 1,
    }
}
