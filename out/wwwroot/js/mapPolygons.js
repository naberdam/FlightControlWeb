/*function ajaxFunc() {
    let xhp = new XMLHttpRequest();
    xhttp.open("GET", "../api/Flights/", true);
    xhttp.send();
    if (this.readyState === 4 && this.status === 200) {

    }
}*/
function showCurrentFlights(str) {
    var xhttp;
    if (str === "") {
        document.getElementById("txtHint").innerHTML = "";
        return;
    }
    xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            document.getElementById("txtHint").innerHTML = this.responseText;
        }
    };
    xhttp.open("GET", "getcustomer.php?q=" + str, true);
    xhttp.send();
}