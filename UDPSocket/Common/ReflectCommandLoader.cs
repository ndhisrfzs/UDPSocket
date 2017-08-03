using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UDPSocket.Common
{
    public class ReflectCommandLoader<TCommand>
        where TCommand : class
    {
        public bool TryLoadCommand(out IEnumerable<TCommand> commands)
        {
            commands = null;
            var assembly = Assembly.GetEntryAssembly();
            var outputCommands = new List<TCommand>();
            try
            {
                outputCommands.AddRange(GetImplementedObjectsByInterface<TCommand>(assembly, typeof(TCommand)));
            }
            catch
            {
                return false;
            }

            commands = outputCommands;
            return true;
        }

        public IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(Assembly assembly, Type targetType)
           where TBaseInterface : class
        {
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TBaseInterface>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;

                if (!targetType.IsAssignableFrom(currentImplementType))
                    continue;

                result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
            }

            return result;
        }
    }
}
