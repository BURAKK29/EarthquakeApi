// Haritayı Türkiye merkez olacak şekilde başlat
var map = L.map('familyMap').setView([39.0, 35.0], 6); // Türkiye'nin ortalama enlem/boylamı
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; OpenStreetMap contributors'
}).addTo(map);

// Model içindeki üyeleri JSON olarak al
// Model.ExistingMembers null ise veya boşsa boş bir diziye çevirerek JavaScript hatasını önle
var members = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(
    Model.ExistingMembers?.Select(m => new {
        m.FirstName,
        m.LastName,
        m.Province,
        m.Country,
        m.Latitude, // Eğer veritabanında varsa bu alanları da gönder
        m.Longitude
    }).ToList() ?? new List < object > () // CS0019 hatası burada düzeltildi: .ToList() eklendi
));

console.log("Yüklendi: Aile Üyeleri", members); // Debug için konsola yazdır

// Harita kapsayıcısının doğru şekilde boyutlandırıldığından emin olun (Leaflet refresh)
// Eğer harita başlangıçta görünmüyorsa bu genellikle yardımcı olur
map.invalidateSize();


// Her bir üye için koordinatları kullan veya Nominatim'dan sorgula ve işaretle
members.forEach(function (m) {
    if (m.Latitude && m.Longitude) {
        // Eğer Latitude ve Longitude veritabanından geldiyse doğrudan kullan
        var lat = parseFloat(m.Latitude);
        var lon = parseFloat(m.Longitude);
        if (!isNaN(lat) && !isNaN(lon)) {
            var marker = L.marker([lat, lon]).addTo(map);
            marker.bindPopup(`<strong>${m.FirstName} ${m.LastName}</strong><br/>${m.Province} / ${m.Country}`);
        }
    } else {
        // Veritabanında koordinat yoksa Nominatim'den çek
        var query = encodeURIComponent(m.Province + ', ' + m.Country);
        fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${query}&limit=1`)
            .then(res => {
                if (!res.ok) {
                    throw new Error(`HTTP hatası! Durum kodu: ${res.status}`);
                }
                return res.json();
            })
            .then(results => {
                if (results && results.length > 0) {
                    var lat = parseFloat(results[0].lat);
                    var lon = parseFloat(results[0].lon);
                    if (!isNaN(lat) && !isNaN(lon)) {
                        var marker = L.marker([lat, lon]).addTo(map);
                        marker.bindPopup(
                            `<strong>${m.FirstName} ${m.LastName}</strong><br/>
                                     ${m.Province} / ${m.Country}`
                        );
                        // OPTIONAL: Eğer coordinates are not saved in DB, you can save them here after fetching.
                        // fetch('/FamilyMembers/UpdateCoordinates', {
                        //     method: 'POST',
                        //     headers: { 'Content-Type': 'application/json' },
                        //     body: JSON.stringify({ Id: m.Id, Latitude: lat, Longitude: lon })
                        // });
                    } else {
                        console.warn(`Geocoding (${m.Province}, ${m.Country}) için geçersiz koordinatlar döndürüldü.`);
                    }
                } else {
                    console.warn(`Geocoding (${m.Province}, ${m.Country}) için sonuç bulunamadı.`);
                }
            })
            .catch(err => console.error(`Geocoding hatası (${m.Province}, ${m.Country}):`, err));
    }
});