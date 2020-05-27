let selected = null;
let markers = [];
let flights = [];
let extMarkers = [];
let extFlights = [];

function getDateTime() {
    let d = new Date();
    let dateTime = d.getFullYear().toString();
    dateTime = dateTime.concat("-");
    let month = d.getMonth() + 1;
    if (month < 10)
        dateTime = dateTime.concat("0");
    dateTime = dateTime.concat(month.toString());
    dateTime = dateTime.concat("-");
    let day = d.getDate();
    if (day < 10)
        dateTime = dateTime.concat("0");
    dateTime = dateTime.concat(day.toString());
    dateTime = dateTime.concat("T");
    if (d.getHours() < 10)
        dateTime = dateTime.concat("0");
    dateTime = dateTime.concat(d.getHours().toString());
    dateTime = dateTime.concat(":");
    if (d.getMinutes() < 10)
        dateTime = dateTime.concat("0");
    dateTime = dateTime.concat(d.getMinutes().toString());
    dateTime = dateTime.concat(":");
    if (d.getSeconds() < 10)
        dateTime = dateTime.concat("0");
    dateTime = dateTime.concat(d.getSeconds().toString());
    dateTime = dateTime.concat("Z");
    return dateTime;
}

async function initializeTable() {
    let table = document.getElementById("intern_table");
    table.innerHTML = "";
    //adding the new flights to the intern_table, moving flight by flight with for-each loop .
    let header = table.createTHead();
    let row = header.insertRow();
    let c0 = row.insertCell(0);
    c0.innerHTML = "ID";
    c0.style.fontWeight = 'bold';
    let c1 = row.insertCell(1);
    c1.innerHTML = "Company";
    c1.style.fontWeight = 'bold';
    let c2 = row.insertCell(2);
    c2.innerHTML = "Passengers";
    c2.style.fontWeight = 'bold';
    let c3 = row.insertCell(3);
    c3.innerHTML = "Delete";
    c3.style.fontWeight = 'bold';
    flights = [];
}

function resetMarkers() {
    let i;
    for (i = 0; i < markers.length; i++) {
        markers[i].setMap(null);
    }
    markers = [];
}

async function initializeExtTable() {
    let table = document.getElementById("extern_table");
    table.innerHTML = "";
    //adding the new flights to the intern_table, moving flight by flight with for-each loop .
    let header = table.createTHead();
    let row = header.insertRow();
    let c0 = row.insertCell(0);
    c0.innerHTML = "ID";
    c0.style.fontWeight = 'bold';
    let c1 = row.insertCell(1);
    c1.innerHTML = "Company";
    c1.style.fontWeight = 'bold';
    let c2 = row.insertCell(2);
    c2.innerHTML = "Passengers";
    c2.style.fontWeight = 'bold';
    /*    let c3 = row.insertCell(3);
        c3.innerHTML = "Delete";
        c3.style.fontWeight = 'bold'*/
    extFlights = [];
}

setInterval(Display, 3000);

async function Display() {
    DisplayAllFlights();
    checkIfSelectedNotNull();
}

async function DisplayAllFlights() {
    //get the date and put in the pattern.
    let dateTime = getDateTime();
    //edit the command
    let flightsUrl = "../api/Flights?relative_to=" + dateTime + "&sync_all";
    let response = await fetch(flightsUrl);
    if ((response.status >= 300 || response.status < 200) && response.status !== 404) {
        alert("Error on server!\n");
    }
    else if (response.status === 404)
        return;
    try {
        let data = await response.json();
        //initialize the flights table (removing the old flights) .
        initializeExtTable();
        initializeTable();
        let counter = 0;
        resetMarkers();
        data.forEach(function (flight) {
            if ((flight.longitude > 180 || flight.longitude < -180) ||
                (flight.latitude > 90 || flight.latitude < -90)) {
                return;
            } else if (flight.is_external === true) {
                updateExternTable(flight);
            } else {
                updateInternTable(flight,counter);
                counter++;
            }
        });
        addEventListnerToRows();
        addEventListnerToExtRows();
    } catch (e) {
        alert("Error in writint extern table.\n");
    }
}
async function updateInternTable(flight,counter) {
    flights.push(flight);
    if (selected !== null && flight.flight_id === selected.flight_id) {
        selected = flight;
        $("#intern_table").append("<tr style=\"background-color: aquamarine\"> <td>"
            + flight.flight_id + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>"
            + flight.passengers + "</td><td><button onmousedown=btnclick(" + counter + ") onclick=event.stopPropagation() onclick=btnclick(this)>"
            + "<img src=\"../images/Trash1.png\"></button></td></tr>");
    } else {
        $("#intern_table").append("<tr style=\"background-color: white\"> <td>"
            + flight.flight_id + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>"
            + flight.passengers + "</td><td><button onmousedown=btnclick(" + counter + ") onclick=event.stopPropagation() >"
            + "<img src=\"../images/Trash1.png\"></button></td></tr>");
    }
    showOnMap(flight);
}
async function updateExternTable(flight){
    if (flight.is_external === true) {
        extFlights.push(flight);
        if (selected !== null && flight.flight_id === selected.flight_id) {
            selected = flight;
            $("#extern_table").append("<tr style=\"background-color: aquamarine\"> <td>"
                + flight.flight_id + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>"
                + flight.passengers + "</td></tr>");
        } else {
            $("#extern_table").append("<tr style=\"background-color: white\"> <td>"
                + flight.flight_id + "</td>" + "<td>" + flight.company_name + "</td>" + "<td>"
                + flight.passengers + "</td></tr>");
        }
        showOnMap(flight);
    }
}
async function checkIfSelectedNotNull() {
    if (selected !== null) {
        if (checkIfSelectedNotEnd() === false) {
            return;
        }
        let table = document.getElementById("tableFlights");
        if (table.rows.length > 1)
            table.deleteRow(1);
        generateTable(selected);
    }
}

function btnclick(numOfRow) {
    let rowCells = document.getElementById("intern_table").rows[numOfRow + 1].cells;
    let id = rowCells[0].innerHTML;
    let m = findMarker(id);
    if (selected !== null && selected.flight_id === id) {
        reset(selected);
        selected = null;
    }
    m.setMap(null);
    document.getElementById("intern_table").deleteRow(numOfRow + 1);
    let url = "../api/Flights/" + id;
    let xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (this.statusText !== "" && (this.status >= 300 || this.status < 200) && this.status !== 404) {
            alert("Error on server!\n");
        }
    };
    xhr.open("DELETE", url, true);
    xhr.send();
}

async function checkIfSelectedNotEnd() {
    if (findExFlight(selected.flight_id) === null && findFlight(selected.flight_id) === null) {
        resetDetailsTable();
        removePath();
        selected = null;
        return false;
    }
    return true;
}

function showOnMap(flight) {
    let icon2 = {
        url: "../images/Travel.png", // url
        scaledSize: new google.maps.Size(40, 40), // scaled size
        origin: new google.maps.Point(0, 0) // origin
    };
    let icon = {
        url: "../images/plane.png", // url
        scaledSize: new google.maps.Size(35, 35), // scaled size
        origin: new google.maps.Point(0, 0) // origin
    };
    let marker;
    if (selected !== null && selected.flight_id === flight.flight_id) {
        marker = new google.maps.Marker({
            position: { lat: flight.latitude, lng: flight.longitude },
            map: map,
            title: flight.flight_id,
            icon: icon2
        });
    }
    else {
        marker = new google.maps.Marker({
            position: { lat: flight.latitude, lng: flight.longitude },
            map: map,
            title: flight.flight_id,
            icon: icon
        });
    }
    markers.push(marker);
    google.maps.event.addListener(marker, 'mousedown', function (event) {
        event.stopPropagation();
    });

    google.maps.event.addListener(marker, 'click', function (marker) {
        let flightsUrl = "../api/FlightPlan/" + flight.flight_id;
        let x = new XMLHttpRequest();
        x.onreadystatechange = function () {
            if (this.readyState === 4 && this.status === 200) {
                let flightPlan = JSON.parse(x.responseText);
                $.ajax(activate(flight, marker, flightPlan));
            }
            else if (this.statusText !== "" && (this.status >= 300 || this.status < 200) && this.status !== 404) {
                alert("Error on server!\n");
            }
        };
        x.open("GET", flightsUrl, true);
        x.send();
    });
}

function helper(flight) {
    let flightsUrl = "../api/FlightPlan/" + flight.flight_id;
    let x = new XMLHttpRequest();
    let id = flight.flight_id;
    let marker = findMarker(id);
    x.onreadystatechange = function (marker) {
        if (this.readyState === 4 && this.status === 200) {
            let flightPlan = JSON.parse(x.responseText);
            $.ajax(activate(flight, marker, flightPlan));
        }
        else if (this.statusText !== "" && (this.status >= 300 || this.status < 200) && this.status !== 404) {
            alert("Error on server!\n");
        }
    };
    x.open("GET", flightsUrl, true);
    x.send();
}

function findMarker(id) {
    let i;
    for (i = 0; i < markers.length; i++) {
        if (markers[i].title === id)
            return markers[i];
    }
}

function findFlight(id) {
    let i;
    for (i = 0; i < flights.length; i++) {
        if (flights[i].flight_id === id)
            return flights[i];
    }
    return null;
}
function findExFlight(id) {
    let i;
    for (i = 0; i < extFlights.length; i++) {
        if (extFlights[i].flight_id === id)
            return extFlights[i];
    }
    return null;
}

function generateTable(flight) {
    let table = document.getElementById("tableFlights");
    let row = table.insertRow(1);
    let c0 = row.insertCell(0);
    c0.innerHTML = flight.flight_id;
    let c1 = row.insertCell(1);
    c1.innerHTML = flight.longitude.toFixed(3);
    let c2 = row.insertCell(2);
    c2.innerHTML = flight.latitude.toFixed(3);
    let c3 = row.insertCell(3);
    c3.innerHTML = flight.passengers;
    let c4 = row.insertCell(4);
    c4.innerHTML = flight.company_name;
    let c5 = row.insertCell(5);
    c5.innerHTML = flight.date_time;
    let c6 = row.insertCell(6);
    c6.innerHTML = flight.is_external;
}

function activate(flight, marker, flightPlan) {
    if (selected !== null) {
        reset(selected);
        if (selected.flight_id === flight.flight_id) {
            selected = null;
            return;
        }
    }
    selected = flight;
    showPath(flightPlan);
    highlightOnTable(flight);
    generateTable(flight);
    changeMarker(marker, flight);
}

function addEventListnerToRows() {
    var table = document.getElementById("intern_table");
    var trList = table.getElementsByTagName("tr");
    for (var index = 0; index < trList.length; index++) {
        (function (index) {
            trList[index].addEventListener("click", function (event) {
                var target = event.target || event.srcElement; //for IE8 backward compatibility
                while (target && target.nodeName !== 'TR') {
                    target = target.parentElement;
                }
                var cells = target.cells; //cells collection
                //var cells = target.getElementsByTagName('td'); //alternative
                if (!cells.length || target.parentNode.nodeName === 'THEAD') { // if clicked row is within thead
                    return;
                }
                let flightId;
                flightId = cells[0].innerHTML;
                let flight = findFlight(flightId);
                helper(flight);
            });
        }(index));
    }
}

function addEventListnerToExtRows() {
    var table = document.getElementById("extern_table");
    var trList = table.getElementsByTagName("tr");
    for (var index = 0; index < trList.length; index++) {
        (function (index) {
            trList[index].addEventListener("click", function (event) {
                var target = event.target || event.srcElement; //for IE8 backward compatibility
                while (target && target.nodeName !== 'TR') {
                    target = target.parentElement;
                }
                var cells = target.cells; //cells collection
                if (!cells.length || target.parentNode.nodeName === 'THEAD') { // if clicked row is within thead
                    return;
                }
                let flightId;
                flightId = cells[0].innerHTML;
                let flight = findExFlight(flightId);
                helper(flight);
            });
        }(index));
    }
}

function changeMarker(marker, flight) {
    let i;
    let x;
    for (i = 0; i < markers.length; i++) {
        if (markers[i].title === flight.flight_id)
            x = i;
    }
    markers[x].setIcon({
        url: "../images/Travel.png", // url
        scaledSize: new google.maps.Size(40, 40), // scaled size
        origin: new google.maps.Point(0, 0) // origin
    });
}

function resetIcon(flight) {
    let i;
    let x;
    for (i = 0; i < markers.length; i++) {
        if (markers[i].title === flight.flight_id)
            x = i;
    }
    markers[x].setIcon({
        url: "../images/plane.png", // url
        scaledSize: new google.maps.Size(35, 35), // scaled size
        origin: new google.maps.Point(0, 0) // origin
    });
}

function highlightOnTable(flight) {
    let table = document.getElementById("intern_table");
    let row;
    for (var i = 0; i < table.rows.length; i++) {
        row = table.rows[i];
        if (row.cells[0].innerHTML === flight.flight_id) {
            row.style.backgroundColor = "aquamarine";
            break;
        }
    }
    table = document.getElementById("extern_table");
    for (i = 0; i < table.rows.length; i++) {
        row = table.rows[i];
        if (row.cells[0].innerHTML === flight.flight_id) {
            row.style.backgroundColor = "aquamarine";
            break;
        }
    }
}

function reset(selected) {
    resetIcon(selected);
    resetDetailsTable();
    resetFlightsTable(selected);
    removePath();
}

function resetFlightsTable(selected) {
    let table = document.getElementById("intern_table");
    let row;
    for (let i = 0; i < table.rows.length; i++) {
        row = table.rows[i];
        if (row.cells[0].innerHTML === selected.flight_id) {
            row.style.backgroundColor = "white";
            break;
        }
    }
    table = document.getElementById("extern_table");
    for (i = 0; i < table.rows.length; i++) {
        row = table.rows[i];
        if (row.cells[0].innerHTML === selected.flight_id) {
            row.style.backgroundColor = "white";
            break;
        }
    }
}

function showPath(flightPlan) {
    flightPlanCoordinates = [];
    let path = flightPath.getPath();
    path = [];
    path.push(new google.maps.LatLng(flightPlan.initial_location.latitude, flightPlan.initial_location.longitude));
    flightPath.setPath(path);
    let i;
    for (i = 0; i < flightPlan.segments.length; i++) {
        path.push(new google.maps.LatLng(flightPlan.segments[i].latitude, flightPlan.segments[i].longitude));
        flightPath.setPath(path);
    }
}

function resetDetailsTable() {
    let table = document.getElementById("tableFlights");
    if (table.rows.length > 1)
        table.deleteRow(1);
}


function removePath() {
    let path = flightPath.getPath();
    path = [];
    flightPath.setPath(path);
}