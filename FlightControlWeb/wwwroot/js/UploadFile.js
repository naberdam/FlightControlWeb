// Empty JS for your own code to be here
function onDrop(ev) {
    ev.preventDefault();
    document.getElementById("dragAndDrop").style.display = "none";
    $("#dragArea").show();
    if (ev.dataTransfer.items[0].kind === 'file') {
        let file = ev.dataTransfer.items[0].getAsFile();
        let flightURL = "../api/FlightPlan";
        let xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (this.statusText !== "" && (this.status >= 300 || this.status < 200) && this.status !== 404) {
                myAlert("Error on server!\n", 2500);
            }
        };
        xhr.open("POST", flightURL, true);
        xhr.setRequestHeader("content-type", "application/json");
        xhr.send(file);
    }
}
function allowDrop(ev) {
    ev.preventDefault();
}
function onDragOver(ev) {
    $("#dragArea").hide();
    $("#dragAndDrop").show();
    ev.preventDefault();
    event.dataTransfer.setData("text/plain", event.target.id);
}

function onDragLeave(ev) {
    ev.preventDefault();
    document.getElementById("dragAndDrop").style.display = "none";
    $("#dragArea").show();
}