var mainBody = document.getElementById("main-body");
var mainButtons = document.getElementById("main-button");
var cancelButtons = document.getElementsByClassName("cancel-button");

var loginForm = document.getElementById("login-form");

document.getElementById("button-login-main").addEventListener("click", function () {

    mainButtons.style.display = "none";
    loginForm.style.display = "block";

});

Array.from(cancelButtons).forEach(element => {
    element.addEventListener("click", function () {
        mainButtons.style.display = "block";
        loginForm.style.display = "none";    
    });
});

loginForm.addEventListener("submit", function (evt) {
    evt.preventDefault();
    var email = loginForm.getElementsByTagName("input")[0].value;
    var password = loginForm.getElementsByTagName("input")[1].value;

    console.log("wo");
    var request = new XMLHttpRequest();
    request.open("GET", `/api/users/login?email=${email}&password=${password}`, true);
    request.send();
    request.onload = function (event) {
        console.log("Received response");

        if (request.status == 200)
        {
            console.log(this.responseText);
        }
    }

    
})