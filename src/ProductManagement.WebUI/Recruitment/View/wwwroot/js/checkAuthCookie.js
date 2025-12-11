window.checkAuthCookie = function () {
    // Check if authentication cookie exists
    var cookies = document.cookie.split(';');
    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i].trim();
        // AspNetCore authentication cookie typically starts with .AspNetCore.Cookies
        if (cookie.startsWith('.AspNetCore.Cookies=') ||
            cookie.startsWith('user_auth=')) {
            console.log('Auth cookie found:', cookie.substring(0, 50) + '...');
            return true;
        }
    }
    console.log('No auth cookie found');
    return false;
};