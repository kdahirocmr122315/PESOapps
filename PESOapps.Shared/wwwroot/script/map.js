window.employerMap = {
    init: function (dotnetHelper, lat, lng) {
        var map = L.map('map').setView([lat || 8.5, lng || 124.6], 10); // Misamis Oriental default

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(map);

        var marker = L.marker([lat || 8.5, lng || 124.6], { draggable: true }).addTo(map);

        // When the marker is moved
        marker.on('dragend', function (e) {
            var position = marker.getLatLng();
            dotnetHelper.invokeMethodAsync('UpdateCoordinates', position.lat, position.lng);
        });

        // Allow user to click on map to move marker
        map.on('click', function (e) {
            marker.setLatLng(e.latlng);
            dotnetHelper.invokeMethodAsync('UpdateCoordinates', e.latlng.lat, e.latlng.lng);
        });
    }
};
