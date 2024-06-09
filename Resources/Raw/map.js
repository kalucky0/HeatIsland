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
const samples = {};

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
        for (let i = 0; i < weather.temperatures.length; i++) {
            const temperature = weather.temperatures[i];
            const pressure = weather.pressures[i];
            let delta = 0;
            if (temperature > 20) {
                delta = temperature - 20;
            }
            updateTileData(obj.id, pressure, temperature, delta, powerCost);
        }
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
    const buildingsFootprint = parseFloat(obj.buildingsFootprint).toFixed(2);
    const averageBuildingHeight = parseFloat(obj.averageBuildingHeight).toFixed(2);
    const energyCost = parseFloat(obj.energyCost).toFixed(2);
    const yearCost = samples[obj.id] ? samples[obj.id].reduce((acc, sample) => acc + sample.energyCost, 0) * 24 : 0;
    const energyCostSaving = samples[obj.id] ? samples[obj.id].reduce((acc, sample) => acc + sample.energyCostSaving, 0) * 24 : 0;
    infoBox.innerHTML = `
            <p><strong>Points:</strong> ${points}</p>
            <p><strong>Vegetation Coverage:</strong> ${vegetationCoverage}%</p>
            <p><strong>Buildings Footprint:</strong> ${buildingsFootprint} m²</p>
            <p><strong>Average Building Height:</strong> ${averageBuildingHeight} m</p>
            <p><strong>Energy Cost:</strong></p>
            <p>&nbsp;&nbsp;&nbsp;${energyCost} PLN per hour</p>
            <p>&nbsp;&nbsp;&nbsp;${yearCost.toFixed(2)} PLN per year</p>
            <p><strong>Potential Energy Savings:</strong></p>
            <p>&nbsp;&nbsp;&nbsp;${obj.energyCostSaving.toFixed(2)} PLN per hour</p>
            <p>&nbsp;&nbsp;&nbsp;${energyCostSaving.toFixed(2)} PLN per year</p>
            <small>Based on ${samples[obj.id] ? samples[obj.id].length : 0} samples</small>
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
                loadFile(fileQueue.pop(), 100800, 26, 0.3, powerCost);
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

function onNewData(id, points, vegetationCoverage, buildingsFootprint, averageBuildingHeight, energy, energyCost, energySaving, energyCostSaving) {
    console.log(id, points, vegetationCoverage, buildingsFootprint, averageBuildingHeight, energy, energyCost, energySaving, energyCostSaving);
    if (samples[id] === undefined) {
        samples[id] = [];
    }
    samples[id].push({ points, vegetationCoverage, buildingsFootprint, averageBuildingHeight, energy, energyCost, energySaving, energyCostSaving });
    let average = { points: 0, vegetationCoverage: 0, buildingsFootprint: 0, averageBuildingHeight: 0, energy: 0, energyCost: 0, energySaving: 0, energyCostSaving: 0 };
    samples[id].forEach(sample => {
        average.points += sample.points;
        average.vegetationCoverage += sample.vegetationCoverage;
        average.buildingsFootprint += sample.buildingsFootprint;
        average.averageBuildingHeight += sample.averageBuildingHeight;
        average.energy += sample.energy;
        average.energyCost += sample.energyCost;
        average.energySaving += sample.energySaving;
        average.energyCostSaving += sample.energyCostSaving;
    });
    average.id = id;
    average.bbox = data[id].bbox;
    average.points /= samples[id].length;
    average.vegetationCoverage /= samples[id].length;
    average.buildingsFootprint /= samples[id].length;
    average.averageBuildingHeight /= samples[id].length;
    average.energy /= samples[id].length;
    average.energyCost /= samples[id].length;
    average.energySaving /= samples[id].length;
    average.energyCostSaving /= samples[id].length;
    updateInfoBox(average);
}

searchButton.addEventListener('click', async () => {
    let response = await fetchCityInfo();

    map.flyTo({
        center: response != null ? [response.lon, response.lat] : [(Math.random() - 0.5) * 360, (Math.random() - 0.5) * 100],
        essential: true,
        zoom: 13
    });
});

document.addEventListener('DOMContentLoaded', async () => loadFile(fileQueue.pop(), 100800, 26, 0.3, powerCost));