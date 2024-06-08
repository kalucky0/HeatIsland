mapboxgl.accessToken = 'pk.eyJ1Ijoia2FsdWNraTIzIiwiYSI6ImNqNHkxMnFzMzFvdGszM2xhYjNycW00YW8ifQ.srmLkTlTXoMc9ZyXPNH-Tw';
const map = new mapboxgl.Map({
    container: 'map',
    // Choose from Mapbox's core styles, or make your own style with Mapbox Studio
    style: 'mapbox://styles/mapbox/standard',
    center: [-120, 50],
    zoom: 2
});

map.on('style.load', () => {
    
    document.getElementById('city').addEventListener('click', () => {
        // if(event.key === 'Enter'){
            fetchCityInfo();
        // }
    });

    async function fetchCityInfo() {
        const city = document.getElementById('city').value;
        if(!city) return;
            
        const response = await fetch(`https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(city)}&format=json&addressdetails=1&limit=1`)
        const data = await response.json();
        
        if (data && data.length > 0) {
            const cityInfo = data[0];
            displayCityInfo(cityInfo);
            return cityInfo;
        }
        return null;
    }

    function displayCityInfo(cityInfo) {
        const cityInfoDiv = document.getElementById('cityInfo');
        cityInfoDiv.innerHTML = `
            <p><strong>Display Name:</strong> ${cityInfo.display_name}</p>
            <p><strong>Latitude:</strong> ${cityInfo.lat}</p>
            <p><strong>Longitude:</strong> ${cityInfo.lon}</p>
            <p><strong>Address:</strong> ${JSON.stringify(cityInfo.address, null, 2)}</p>
        `;
    }

    document.getElementById('fly').addEventListener('click', async () => {
        let response = await fetchCityInfo();
        
        map.flyTo({
            center: response != null ? [response.lon, response.lat] : [(Math.random() - 0.5) * 360, (Math.random() - 0.5) * 100],
            essential: true,
            zoom: 10
        });
    });
});
