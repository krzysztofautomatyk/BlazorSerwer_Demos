window.setCultureCookieAndReload = function (cookieValue) {
    document.cookie = '.AspNetCore.Culture=' + cookieValue + '; path=/; max-age=31536000'; // max-age=1 year
    window.location.reload();
};
