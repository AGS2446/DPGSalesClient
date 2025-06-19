var map = null;

$.post('GetJsonUserGps').done(function (gps) {
    debugger;
    if (gps) {
        if (gps.length) {
            var image = {
                url: '../images/gps/ic_start.png',
                // This marker is 20 pixels wide by 32 pixels tall.
                size: new google.maps.Size(36, 46),
                // The origin for this image is 0,0.
                origin: new google.maps.Point(0, 0),
                // The anchor for this image is the base of the flagpole at 0,32.
                anchor: new google.maps.Point(18, 51)
            };
            var mapOptions = {
                center: new google.maps.LatLng(gps[0].latitude, gps[0].longitude),
                zoom: 15,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            map = new google.maps.Map(document.getElementById("map-users"), mapOptions);
            var latlngbounds = new google.maps.LatLngBounds();
            for (var intMarkers = 0; intMarkers < gps.length; intMarkers++) {
                latlngbounds.extend(new google.maps.LatLng(gps[intMarkers].latitude, gps[intMarkers].longitude));
                var marker = new MarkerWithLabel({
                    position: new google.maps.LatLng(gps[intMarkers].latitude, gps[intMarkers].longitude),
                    draggable: false,
                    map: map,
                    labelContent: '',
                    labelAnchor: new google.maps.Point(5, 0),
                    labelClass: "marker-label", // the CSS class for the label
                    labelStyle: { opacity: 1 },
                    icon: image
                });
                marker.setTitle(gps[intMarkers].name);
                attachInfoMessage(marker, gps[intMarkers].address, gps[intMarkers].updatedOn);
            }
            map.fitBounds(latlngbounds);
        }
        else {
            getCurrentPosition('map-users');
        }
    }
    else {
        getCurrentPosition('map-users');
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