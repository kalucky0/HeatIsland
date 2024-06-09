const cityInput = document.getElementById('city');
const searchButton = document.getElementById('fly');
const progressDialog = document.getElementById('loader');
const progressBar = document.querySelector('.progress');
const progressPercent = document.querySelector('.info p');
const progressItems = document.querySelectorAll('.info p')[1];
const infoBox = document.getElementById('info');

const fileQueue = [
    "D:\\Libraries\\Downloads\\78989_1475484_M-34-64-D-d-2-1-3-3.laz",
    "D:\\Libraries\\Downloads\\78989_1475609_M-34-64-D-d-1-2-4-4.laz",
    "D:\\Libraries\\Downloads\\78989_1475631_M-34-64-D-d-1-4-2-2.laz",
    "D:\\Libraries\\Downloads\\78989_1475648_M-34-64-D-d-2-3-1-1.laz"
];
const powerCost = 1.12;

const data = {};

mapboxgl.accessToken = 'pk.eyJ1Ijoia2FsdWNraTIzIiwiYSI6ImNqNHkxMnFzMzFvdGszM2xhYjNycW00YW8ifQ.srmLkTlTXoMc9ZyXPNH-Tw';
const map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/dark-v11',
    center: [19.1451, 51.9194],
    zoom: 4
});

map.on('load', () => {
    map.fitBounds([
        [14.0745211117, 49.0273953314],
        [24.0299857927, 54.8515359564]
    ]);
});

map.on('click', async (e) => {
    const position = e.lngLat;
    const obj = Object.values(data).find((value) => {
        const bbox = value.bbox;
        console.log(position.lng, bbox[1], bbox[3], position.lat, bbox[0], bbox[2]);
        return position.lng >= bbox[1] && position.lng <= bbox[3] && position.lat >= bbox[0] && position.lat <= bbox[2];
    });
    if (obj) {
        const bbox = obj.bbox;
        map.fitBounds([
            [bbox[1], bbox[0]],
            [bbox[3], bbox[2]]
        ]);

        updateInfoBox(obj);

        const weather = await getWeather();
        let costs = [];
        let savings = [];
        for (let i = 0; i < weather.temperatures.length; i++) {
            const temperature = weather.temperatures[i];
            const pressure = weather.pressures[i];
            let delta = 0;
            if (temperature > 20) delta = temperature - 20;
            const energy = calculateEnergy(obj.buildingsFootprint * obj.averageBuildingHeight, pressure, temperature, delta);
            costs.push(calculateEnergyCost(energy, powerCost));
            if (delta > 0) {
                const savedEnergy = calculateEnergy(obj.buildingsFootprint * obj.averageBuildingHeight, pressure, temperature, 0.29);
                savings.push(calculateEnergyCost(savedEnergy, powerCost));
            } 
        }
        obj.energyCost = costs.reduce((acc, value) => acc + value, 0) / costs.length;
        obj.energyCostYear = obj.energyCost * 24 * 365;
        obj.energyCostSaved = savings.reduce((acc, value) => acc + value, 0) / savings.length;
        obj.energyCostSavedYear = obj.energyCostSaved * 24 * 365;
        updateInfoBox(obj);
    }
});

async function fetchCityInfo() {
    const city = cityInput.value;
    if (!city) return;

    const response = await fetch(`https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(city)}&format=json&addressdetails=1&limit=1`)
    const data = await response.json();

    if (data && data.length > 0) {
        const cityInfo = data[0];
        displayCityInfo(cityInfo);
        return cityInfo;
    }
    return null;
}

function updateInfoBox(obj) {
    console.log(obj);
    const points = parseFloat(obj.points).toFixed(1);
    const vegetationCoverage = parseFloat(obj.vegetationCoverage).toFixed(3);
    const buildingsFootprint = parseFloat(obj.buildingsFootprint);
    const averageBuildingHeight = parseFloat(obj.averageBuildingHeight);
    const color = `color: ${vegetationCoverage > 0.2 ? '#66bb6a' : vegetationCoverage > 0.1 ? '#ff9800' : '#f25757'}`;
    infoBox.innerHTML = `
            <p><strong>Points:</strong> ${points}</p>
            <p><strong>Vegetation Coverage:</strong> <span style="${color}">${vegetationCoverage}%</span></p>
            <p><strong>Buildings Footprint:</strong> ${buildingsFootprint.toFixed(2)} m²</p>
            <p><strong>Average Building Height:</strong> ${averageBuildingHeight.toFixed(2)} m</p>
            <p><strong>Energy Cost:</strong></p>
            <p>&nbsp;&nbsp;&nbsp;${obj.energyCost ? obj.energyCost.toFixed(2) : 0} PLN per hour</p>
            <p>&nbsp;&nbsp;&nbsp;${obj.energyCostYear ? obj.energyCostYear.toFixed(2) : 0} PLN per year</p>
            <p><strong>+1% in Vegetation Coverage:</strong></p>
            <p>&nbsp;&nbsp;&nbsp;-${(obj.energyCostSaved ? obj.energyCostSaved.toFixed(2) : 0)} PLN per hour</p>
            <p>&nbsp;&nbsp;&nbsp;-${(obj.energyCostSavedYear ? obj.energyCostSavedYear.toFixed(2) : 0)} PLN per year</p>
        `;
}

function addImage(id, dataUri, minLon, minLat, maxLon, maxLat, points, vegetationCoverage, buildingsFootprint, averageBuildingHeight, energy, energyCost, energySaving, energyCostSaving) {
    data[id] = { id: id, bbox: [minLon, minLat, maxLon, maxLat], points, vegetationCoverage, buildingsFootprint, averageBuildingHeight, energy, energyCost, energySaving, energyCostSaving };
    console.log(data[id]);
    map.addSource(`${id}`, {
        'type': 'image',
        'url': dataUri,
        'coordinates': [
            [minLat, maxLon],
            [maxLat, maxLon],
            [maxLat, minLon],
            [minLat, minLon]
        ]
    });
    map.addLayer({
        id: id + '-layer',
        'type': 'raster',
        'source': `${id}`,
        'paint': {
            'raster-fade-duration': 500
        }
    });
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

function updateProgress(items, total) {
    progressDialog.style.display = 'flex';
    const percent = Math.round((items / total) * 100);
    progressBar.style.width = percent + '%';
    progressPercent.textContent = percent + '%';
    progressItems.textContent = items + '/' + total;
    if (items >= total) {
        setTimeout(() => {
            progressDialog.style.display = 'none';
            if (fileQueue.length > 0) {
                loadFile(fileQueue.pop(), 100800, 26, 0.29, powerCost);
            }
        }, 1000);
    }
}

function loadFile(path, pressure, temperature, temperatureDelta, costPerKW) {
    callCSharp('Ready', [path, pressure, temperature, temperatureDelta, costPerKW]);
}

function updateTileData(id, pressure, temperature, temperatureDelta, costPerKW) {
    callCSharp('GetData', [id, pressure, temperature, temperatureDelta, costPerKW]);
}

searchButton.addEventListener('click', async () => {
    let response = await fetchCityInfo();

    map.flyTo({
        center: response != null ? [response.lon, response.lat] : [(Math.random() - 0.5) * 360, (Math.random() - 0.5) * 100],
        essential: true,
        zoom: 13
    });
});

document.addEventListener('DOMContentLoaded', async () => loadFile(fileQueue.pop(), 100800, 26, 0.29, powerCost));