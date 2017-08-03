using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPSocket.Command;
using UDPSocket.Common;
using UDPSocket.Engine;
using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket.Server
{
    public abstract class AppServerBase<TAppSession, TRequestInfo> : IAppServer<TAppSession, TRequestInfo>, IRequestHandler<TRequestInfo>, ISocketServerAccessor, IDisposable
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        public DateTime StartedTime { get; private set; }

        private Dictionary<UInt16, CommandInfo<ICommand<TAppSession, TRequestInfo>>> m_CommandContainer;

        private ISocketServer m_SocketServer;
        ISocketServer ISocketServerAccessor.SocketServer
        {
            get { return m_SocketServer; }
        }

        IAppSession IAppServer.CreateAppSession(ISocketSession socketSession)
        {
            var appSession = CreateAppSession(socketSession);
            appSession.Initialize(this, socketSession);

            return appSession;
        }

        public virtual TAppSession CreateAppSession(ISocketSession socketSession)
        {
            return new TAppSession();
        }

        public abstract TAppSession GetSessionByID(ulong sessionID);
        IAppSession IAppServer.GetSessionByID(ulong sessionID)
        {
            return this.GetSessionByID(sessionID);
        }
        public abstract bool RegisterSession(TAppSession appSession);
        bool IAppServer.RegisterSession(IAppSession session)
        {
            var appSession = session as TAppSession;
            if (!RegisterSession(appSession))
                return false;

            appSession.SocketSession.Closed += OnSocketSessionClosed;

            //OnNewSessionConnected(appSession);
            return true;
        }

        private void OnSocketSessionClosed(ISocketSession session, CloseReason reason)
        {
            Console.WriteLine("Close");
            var appSession = session.AppSession as TAppSession;
            appSession.Connected = false;
            OnSessionClosed(appSession, reason);
        }
        protected virtual void OnSessionClosed(TAppSession session, CloseReason reason)
        {
            session.OnSessionClosed(reason);
        }
        public abstract IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera);
        public abstract IEnumerable<TAppSession> GetAllSessions();

        public bool Setup(int port)
        {
            if (!SetupAdvanced())
                return false;

            if(!SetupSocketServer(port))
            {
                return false;
            }

            return true;
        }

        private bool SetupSocketServer(int port)
        {
            try
            {
                m_SocketServer = new UdpSocketServer<TRequestInfo>(this, port);
                return m_SocketServer != null;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public virtual bool Start()
        {
            if(!m_SocketServer.Start())
            {
                return false;
            }
            return false;
        }

        private bool SetupAdvanced()
        {
            var discoveredCommands = new Dictionary<UInt16, ICommand<TAppSession, TRequestInfo>>();
            if (!SetupCommands(discoveredCommands))
                return false;

            OnCommandSetup(discoveredCommands);

            return true;
        }

        private bool SetupCommands(Dictionary<UInt16, ICommand<TAppSession, TRequestInfo>> discoveredCommands)
        {
            var loader = new ReflectCommandLoader<ICommand<TAppSession, TRequestInfo>>();
            IEnumerable<ICommand<TAppSession, TRequestInfo>> commands;
            if (!loader.TryLoadCommand(out commands))
            {
                return false;
            } 

            if(commands != null && commands.Any())
            {
                foreach (var c in commands)
                {
                    if(discoveredCommands.ContainsKey(c.ID))
                    {
                        return false;
                    }
                    var castedCommand = c as ICommand<TAppSession, TRequestInfo>;
                    if(castedCommand == null)
                    {
                        return false;
                    }

                    discoveredCommands.Add(c.ID, castedCommand);
                }
            }

            return true;
        }

        private void OnCommandSetup(IDictionary<UInt16, ICommand<TAppSession, TRequestInfo>> discoveredCommands)
        {
            var commandContainer = new Dictionary<UInt16, CommandInfo<ICommand<TAppSession, TRequestInfo>>>();
            foreach (var command in discoveredCommands.Values)
            {
                commandContainer.Add(command.ID, new CommandInfo<ICommand<TAppSession, TRequestInfo>>(command));
            }

            Interlocked.Exchange(ref m_CommandContainer, commandContainer);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public virtual void ExecuteCommand(TAppSession session, TRequestInfo requestInfo)
        {
            var commandProxy = GetCommandByID(requestInfo.key);
            if(commandProxy != null)
            {
                var command = commandProxy.Command;
                command.ExecuteCommand(session, requestInfo);
            }

            session.LastActiveTime = DateTime.Now;
        }

        void IRequestHandler<TRequestInfo>.ExecuteCommand(IAppSession session, TRequestInfo requestInfo)
        {
            this.ExecuteCommand((TAppSession)session, requestInfo);
        }

        private CommandInfo<ICommand<TAppSession, TRequestInfo>> GetCommandByID(UInt16 id)
        {
            CommandInfo<ICommand<TAppSession, TRequestInfo>> commandProxy;
            if (m_CommandContainer.TryGetValue(id, out commandProxy))
                return commandProxy;
            else
                return null;
        }

        IEnumerable<TAppSession> IAppServer<TAppSession>.GetSessions(Func<TAppSession, bool> critera)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TAppSession> IAppServer<TAppSession>.GetAllSessions()
        {
            throw new NotImplementedException();
        }

       
    }
}
