using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Service
{
    public class SessionTimerService
    {
        private DateTime _lastActivityTime;
        private bool _isSessionActive = false;

        public void StartSession()
        {
            _isSessionActive = true;
            _lastActivityTime = DateTime.UtcNow;
            Console.WriteLine($"[SessionTimer] Session started");
        }

        public void ResetSession()
        {
            if (_isSessionActive)
            {
                _lastActivityTime = DateTime.UtcNow;
                Console.WriteLine($"[SessionTimer] Session reset");
            }
        }

        public void StopSession()
        {
            _isSessionActive = false;
            Console.WriteLine($"[SessionTimer] Session stopped");
        }

    }
}
