$('#Search').datepicker({
    format: 'dd/mm/yyyy'
});

var map = null;

$.getJSON("GetJsonUserNavGps", { UserId: $('#UserId').val(), Search: $('#Search').val() }, function (data) {
    debugger;
    if (data) {
        if (data.length) {

            var imageStart = {
                url: '../images/gps/ic_starte.png',
                // This marker is 20 pixels wide by 32 pixels tall.
                size: new google.maps.Size(36, 46),
                // The origin for this image is 0,0.
                origin: new google.maps.Point(0, 0),
                // The anchor for this image is the base of the flagpole at 0,32.
                anchor: new google.maps.Point(18, 51)
            };

            var imageEnd = {
                url: '../images/gps/ic_ende.png',
                // This marker is 20 pixels wide by 32 pixels tall.
                size: new google.maps.Size(36, 46),
                // The origin for this image is 0,0.
                origin: new google.maps.Point(0, 0),
                // The anchor for this image is the base of the flagpole at 0,32.
                anchor: new google.maps.Point(18, 51)
            };


            var directionsService = new google.maps.DirectionsService;
            var directionsDisplay = new google.maps.DirectionsRenderer;
            var map = new google.maps.Map(document.getElementById('map-user-navigation'), {
                zoom: 8,
                center: new google.maps.LatLng(data[0].latitude, data[0].longitude)
            });

            var waypts = [];            
            var start = new google.maps.LatLng(data[0].latitude, data[0].longitude);
            var end = new google.maps.LatLng(data[data.length - 1].latitude, data[data.length - 1].longitude);
            var breakVal = parseInt(data.length / 8);
            for (var i = data.length - 2; i > 0; i--) {
                if (i % breakVal == 0)
                    waypts.push({
                        location: new google.maps.LatLng(data[i].latitude, data[i].longitude),
                        stopover: true
                    });
            }

            var markerStart = new MarkerWithLabel({
                position: new google.maps.LatLng(data[0].latitude, data[0].longitude),
                draggable: false,
                map: map,
                labelContent: '',
                labelAnchor: new google.maps.Point(5, 0),
                labelClass: "marker-label", // the CSS class for the label
                labelStyle: { opacity: 1 },
                icon: imageStart
            });
            markerStart.setTitle("Start Point");
            attachInfoMessage(markerStart, data[0].address, data[0].createdOn);

            var markerEnd = new MarkerWithLabel({                
                position: new google.maps.LatLng(data[data.length - 1].latitude, data[data.length - 1].longitude),
                draggable: false,
                map: map,
                labelContent: '',
                labelAnchor: new google.maps.Point(5, 0),
                labelClass: "marker-label", // the CSS class for the label
                labelStyle: { opacity: 1 },
                icon: imageEnd
            });
            markerEnd.setTitle("End Point");
            attachInfoMessage(markerEnd, data[data.length - 1].address, data[data.length - 1].createdOn);
            


            debugger;
            directionsDisplay.setMap(map);
            //var latlngbounds = new google.maps.LatLngBounds();
            //map.fitBounds(latlngbounds);
            calculateAndDisplayRoute(directionsService, directionsDisplay, start, end, waypts, name);
        }
        else {
            getCurrentPosition('map-user-navigation');
        }
    }
    else {
        getCurrentPosition('map-user-navigation');
    }
});


function getCurrentPosition(elementid) {
    navigator.geolocation.getCurrentPosition(function (position) {
        var mapOptions = {
            center: new google.maps.LatLng(position.coords.latitude, position.coords.longitude),
            zoom: 15,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        map = new google.maps.Map(document.getElementById(elementid), mapOptions);
    });
}

function attachInfoMessage(marker, Location, Time) {
    debugger;
    var contentString = '<div class="info-window">' +
            '<div class="info-window-heading">' + marker.title + '</div>' +
            '<div class="info-window-sub-heading m-t-xs">Location: </div>' +
            '<div class="info-window-body">' + Location + '</div>' +
            '<div class="info-window-sub-heading m-t-xs">Time: </div>' +
            '<div class="info-window-body">' + Time + '</div>' +
            //'<div class="info-window-body">Lat: ' + marker.position.lat() + ', Lng:' + marker.position.lng() + '</div>' +

            '</div>';

    var infowindow = new google.maps.InfoWindow({
        content: contentString
    });

    google.maps.event.addListener(marker, 'mouseover', function () {
        debugger;
        infowindow.open(marker.get('map'), marker);
    });

    google.maps.event.addListener(marker, 'mouseout', function () {
        debugger;
        infowindow.close(marker.get('map'), marker);
    });
}

function calculateAndDisplayRoute(directionsService, directionsDisplay, start, end, waypts, name) {
    debugger;
    if (waypts.length > 8) {
        waypts.splice(8, waypts.length - 8);
    }
    directionsService.route({
        origin: start,
        destination: end,
        waypoints: waypts,
        optimizeWaypoints: true,
        travelMode: 'DRIVING'
    }, function (response, status) {
        debugger;
        if (status === 'OK') {
            directionsDisplay.setDirections(response);
            directionsDisplay.setOptions({ suppressMarkers: true });

            //var route = response.routes[0];
            //var summaryPanel = document.getElementById('directions-panel');
            //summaryPanel.innerHTML = '';
            //// For each route, display summary information.
            //for (var i = 0; i < route.legs.length; i++) {
            //    var routeSegment = i + 1;
            //    summaryPanel.innerHTML += '<b>Route Segment: ' + routeSegment +
            //        '</b><br>';
            //    summaryPanel.innerHTML += route.legs[i].start_address + ' to ';
            //    summaryPanel.innerHTML += route.legs[i].end_address + '<br>';
            //    summaryPanel.innerHTML += route.legs[i].distance.text + '<br><br>';
            //}
        } else {
            window.alert('Directions request failed due to ' + status);
        }
    });
}
