// wwwroot/js/familyMapInitializer.js

function initializeFamilyMapWithMembers(membersData) {
    const mapContainer = document.getElementById('familyMap');
    if (!mapContainer) return;

    const map = L.map('familyMap').setView([39.0, 35.0], 6);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors',
        maxZoom: 18
    }).addTo(map);

    // Özel koyu renkli marker ikonu
    const customIcon = L.divIcon({
        className: '',
        html: `<div style="
            width:32px;height:32px;
            background:linear-gradient(135deg,#3b82f6,#06b6d4);
            border-radius:50% 50% 50% 0;
            transform:rotate(-45deg);
            border:3px solid #fff;
            box-shadow:0 3px 12px rgba(59,130,246,.6);
        "></div>`,
        iconSize: [32, 32],
        iconAnchor: [16, 32],
        popupAnchor: [0, -34]
    });

    // Case-insensitive + Türkçe karakter normalize lookup
    function normalizeProvince(str) {
        if (!str) return '';
        return str
            .toLowerCase()
            .replace(/ı/g, 'i')
            .replace(/ğ/g, 'g')
            .replace(/ü/g, 'u')
            .replace(/ş/g, 's')
            .replace(/ö/g, 'o')
            .replace(/ç/g, 'c')
            .trim();
    }

    // provinceCoordinates key'lerini normalize edilmiş formata eşle
    function findCoords(province) {
        if (!province || typeof provinceCoordinates === 'undefined') return null;
        const normInput = normalizeProvince(province);
        for (const [key, coords] of Object.entries(provinceCoordinates)) {
            if (normalizeProvince(key) === normInput) return coords;
        }
        return null;
    }

    let markersAdded = 0;
    const bounds = [];

    membersData.forEach(member => {
        let lat = null, lon = null;

        // Önce veritabanı koordinatları
        if (member.Latitude && member.Longitude) {
            lat = parseFloat(member.Latitude);
            lon = parseFloat(member.Longitude);
            if (isNaN(lat) || isNaN(lon)) { lat = null; lon = null; }
        }

        // Yoksa il adından bul
        if (lat === null) {
            const coords = findCoords(member.Province);
            if (coords) { lat = coords[0]; lon = coords[1]; }
        }

        if (lat !== null && lon !== null) {
            const marker = L.marker([lat, lon], { icon: customIcon }).addTo(map);
            marker.bindPopup(`
                <div style="font-family:'Inter',sans-serif;min-width:140px">
                    <strong style="color:#1e293b">${member.FirstName} ${member.LastName}</strong>
                    <hr style="margin:4px 0;border-color:#e2e8f0">
                    <span style="color:#475569">📍 ${member.Province}</span><br>
                    <span style="color:#94a3b8;font-size:.85em">${member.Country}</span>
                </div>
            `);
            bounds.push([lat, lon]);
            markersAdded++;
        } else {
            console.warn('Koordinat bulunamadı:', member.Province);
        }
    });

    // Eğer marker varsa haritayı onlara odakla
    if (bounds.length > 0) {
        map.fitBounds(bounds, { padding: [40, 40] });
    }

    // Layout sonrası haritayı yeniden boyutlandır
    setTimeout(() => map.invalidateSize(), 200);
}