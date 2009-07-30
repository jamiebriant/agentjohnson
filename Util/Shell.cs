namespace AgentJohnson.Util
{
    using System;
    using EnvDTE;
    using JetBrains.Annotations;
    using JetBrains.VsIntegration.Application;
    
    public static class Shell
    {
        [NotNull]
        public static _DTE GetDte()
        {
            return VSShell.Instance.ServiceProvider.Dte();
        }

        public static void CreateCommand([NotNull] string name, bool execute, [CanBeNull] Action<Command> action)
        {
            _DTE dte = GetDte();

            Command command;
            try
            {
                command = dte.Commands.Item(name, -1);
            }
            catch
            {
                command = null;
            }

            if (execute && command != null)
            {
                dte.ExecuteCommand(name, string.Empty);
            }

            if (action != null)
            {
                action(command);
            }
        }
    }
}
