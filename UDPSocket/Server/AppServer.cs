using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPSocket.Protocol;

namespace UDPSocket.Server
{
    abstract class AppServer<TAppSession, TRequestInfo> : AppServerBase<TAppSession, TRequestInfo>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, new()
    {
        private ConcurrentDictionary<UInt64, TAppSession> m_SessionDict = new ConcurrentDictionary<ulong, TAppSession>();
        private KeyValuePair<UInt64, TAppSession>[] SessionSource
        {
            get
            {
                return m_SessionDict.ToArray();
            }
        }

        public override IEnumerable<TAppSession> GetAllSessions()
        {
            var sessionSource = SessionSource;
            if (sessionSource == null)
                return null;

            return sessionSource.Select(c => c.Value);
        }

        public override TAppSession GetSessionByID(ulong sessionID)
        {
            TAppSession targetSession;
            m_SessionDict.TryGetValue(sessionID, out targetSession);
            return targetSession;
        }

        public override IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
        {
            var sessionSource = SessionSource;
            if (sessionSource == null)
                return null;

            return sessionSource.Select(c => c.Value).Where(critera);
        }

        public override bool RegisterSession(TAppSession appSession)
        {
            if (m_SessionDict.TryAdd(appSession.SessionID, appSession))
                return true;

            return false;
        }

        protected override void OnSessionClosed(TAppSession session, CloseReason reason)
        {
            TAppSession removedSession;
            if(!m_SessionDict.TryRemove(session.SessionID, out removedSession))
            {
               //log 
            }
            base.OnSessionClosed(session, reason);
        }
    }
}
