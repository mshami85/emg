var LeafIcon = L.Icon.extend({
    options: {
        shadowUrl: '/images/marker-shadow.png',
        shadowSize: [25, 50],
        shadowAnchor: [6, 49],

        iconSize: [25, 41],
        iconAnchor: [12, 40],
        popupAnchor: [0, -25]
    }
});
var greenIcon = new LeafIcon({ iconUrl: '/images/green-mark.png' });
var darkIcon = new LeafIcon({ iconUrl: '/images/dark-mark.png' });
var lightgreenIcon = new LeafIcon({ iconUrl: '/images/lightgreen-mark.png' });
var redIcon = new LeafIcon({ iconUrl: '/images/red-mark.png' });
var violetIcon = new LeafIcon({ iconUrl: '/images/violet-mark.png' });
var orangeIcon = new LeafIcon({ iconUrl: '/images/orange-mark.png' });

var osm = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap',
});
var osmHOT = L.tileLayer('https://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap',
});
var openTopoMap = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
    maxZoom: 17,
    attribution: '© OpenStreetMap'
});
var baseMaps = {
    "OpenStreetMap": osm,
    "OpenStreetMap.HOT": osmHOT,
    "OpenTopoMap": openTopoMap
};

var map = L.map('map', {
    zoom: 10,
    layers: [osm],
});

map.attributionControl.setPrefix('سلامة');
var layerControl = L.control.layers(baseMaps).addTo(map);

function addMobileStatusMarker(status, lat, lng, mobile, text) {
    var ico;
    if (status == 0) {
        ico = greenIcon;
    }
    else if (status == 1) {
        ico = orangeIcon;
    }
    else {
        ico = redIcon;
    }
    var m = L.marker([lat, lng], { icon: ico }).addTo(map).bindPopup("<b>" + mobile + "</b><br/>" + text);
    return m;
}

function addAdminMessageMarker(lat, lng, text) {
    return L.marker([lat, lng], { icon: violetIcon }).addTo(map).bindPopup(text);
}

function loadLatestStatues() {
    $.ajax({
        url: '/Mobile/GetLatestStatues',
        type: "GET",
        success: function (json) {
            var data = JSON.parse(json);
            var marks = [];
            for (var i = 0; i < data.length; i++) {
                var m = addMobileStatusMarker(data[i].Status, data[i].Location.Latitude, data[i].Location.Longitude, data[i].Mobile.Owner, data[i].Text);
                marks.push(m);
            }
            if (marks.length == 1) {
                var last = data[data.length - 1];
                if (last) {
                    map.panTo([last.Location.Latitude, last.Location.Longitude], { animate: true });
                    map.setZoom(12);
                }
            }
            else if (marks.length > 1) {
                var group = L.featureGroup(marks);
                map.fitBounds(group.getBounds());
            }
            else {
                map.locate({ setView: true });
            }
        }
    });
}

/*
 var popup = L.popup();
 function onMapClick(e) {
     popup.setLatLng(e.latlng)
          .setContent("You clicked the map at " + e.latlng.toString())
          .openOn(map);
 }
 map.on('click', onMapClick);
 */