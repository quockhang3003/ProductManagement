window.sessionManager = (function () {
    let dotnetHelper = null;
    let timeoutMinutes = 10;
    let warningBeforeMinutes = 1;
    let warningTimer = null;
    let logoutTimer = null;
    let started = false;

    function log(msg, obj) {
        console.log(`[sessionManager] ${msg}`, obj || '');
    }

    function init(dotnetRef) {
        dotnetHelper = dotnetRef;
        log("init called", { dotnetRefExists: !!dotnetRef });
    }

    function start(timeoutMin, warnBeforeMin) {
        if (started) {
            log("already started");
            return;
        }

        timeoutMinutes = Number(timeoutMin) || 10;
        warningBeforeMinutes = Number(warnBeforeMin) || 1;

        if (warningBeforeMinutes >= timeoutMinutes) {
            warningBeforeMinutes = Math.max(1, timeoutMinutes - 1);
        }

        started = true;
        resetTimers();

        log("started timers", {
            timeoutMinutes,
            warningBeforeMinutes,
            warnAfterMs: (timeoutMinutes - warningBeforeMinutes) * 60 * 1000,
            logoutAfterMs: timeoutMinutes * 60 * 1000
        });
    }

    function stop() {
        started = false;
        clearTimeout(warningTimer);
        warningTimer = null;
        clearTimeout(logoutTimer);
        logoutTimer = null;
        hideModal();
        log("stopped timers");
    }

    function resetTimers() {
        if (!started) {
            log("resetTimers aborted: session not started");
            return;
        }

        clearTimeout(warningTimer);
        clearTimeout(logoutTimer);

        const warnAfterMs = Math.max(0, (timeoutMinutes - warningBeforeMinutes) * 60 * 1000);
        const logoutAfterMs = Math.max(0, timeoutMinutes * 60 * 1000);

        log("setting timers", { warnAfterMs, logoutAfterMs });

        if (warnAfterMs > 0) {
            warningTimer = setTimeout(() => {
                log("warningTimer triggered");
                showWarningModal();
            }, warnAfterMs);
        }
        if (logoutAfterMs > 0) {
            logoutTimer = setTimeout(() => {
                log("logoutTimer triggered");
                expireSession();
            }, logoutAfterMs);
        }

        log("timers set", {
            warnAfterMs,
            logoutAfterMs,
            now: new Date().toLocaleTimeString()
        });
    }

    function showWarningModal() {
        const modal = document.getElementById('session-timeout-modal');
        if (!modal) {
            console.error("[sessionManager] Modal element not found in DOM");
            return;
        }
        if (modal.style.display === 'flex') {
            console.warn("[sessionManager] Modal already visible");
            return;
        }
        modal.classList.add('show');
        modal.style.display = 'flex';
        log("showWarningModal triggered successfully", { time: new Date().toLocaleTimeString(), modalStyle: modal.style.display });
    }

    function hideModal() {
        const modal = document.getElementById('session-timeout-modal');
        if (modal) {
            modal.classList.remove('show');
            modal.style.display = 'none';
            log("hideModal called");
        }
    }

    function expireSession() {
        log("expireSession called", new Date().toLocaleTimeString());
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync("LogoutNowAsync")
                .then(() => log("LogoutNowAsync called successfully"))
                .catch(e => console.error("expire -> LogoutNowAsync failed", e));
        } else {
            console.error("[sessionManager] dotnetHelper is null - falling back to client-side logout");
            localStorage.removeItem("isLoggedIn");
            window.location.assign("/login");
        }
        hideModal();
        stop();
    }

    function keepWorking() {
        log("keepWorking called", new Date().toLocaleTimeString());
        hideModal();
        resetTimers();
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync("ResetSessionAsync")
                .catch(e => console.error("keepWorking -> ResetSessionAsync failed", e));
        }
    }

    function onApiSuccess() {
        if (!started) {
            log("onApiSuccess ignored: session not started");
            return;
        }
        log("API call successful - resetting timers", new Date().toLocaleTimeString());
        resetTimers();
    }

    return {
        init,
        start,
        stop,
        keepWorking,
        closeSession: expireSession,
        onApiSuccess
    };
})();


