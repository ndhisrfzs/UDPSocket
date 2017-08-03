namespace UDPSocket.Command
{
    class CommandInfo<TCommand>
        where TCommand : ICommand
    {
        public TCommand Command { get; private set; }

        public CommandInfo(TCommand command)
        {
            Command = command;
        }
    }
}
