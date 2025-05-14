namespace Moodle_Migration.Interfaces
{
    public interface ICommandProcessor
    {
        /// <summary>
        /// Processes a command based on the input arguments.
        /// </summary>
        /// <param name="args">An array of command-line arguments.</param>
        /// <returns>A task representing the asynchronous operation, with a result containing the command output.</returns>
        Task<string> ProcessCommand(string[] args);
    }

}
