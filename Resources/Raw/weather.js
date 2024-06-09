async function getWeather() {
    const latitude = 50.0614;
    const longitude = 19.9366;
    const startDate = '2023-01-01';
    const endDate = '2023-12-31';
    const hourly = 'temperature_2m,surface_pressure';
    const timezone = 'auto';
    const url = `https://archive-api.open-meteo.com/v1/archive?latitude=${latitude}&longitude=${longitude}&start_date=${startDate}&end_date=${endDate}&hourly=${hourly}&timezone=${timezone}`;
    const response = await fetch(url);
    const data = await response.json();
    const temperatures = data.hourly.temperature_2m;
    const pressures = data.hourly.surface_pressure;

    let daily = {
        temperatures: [],
        pressures: []
    };

    for (let i = 0; i < temperatures.length; i += 24) {
        let max = -Infinity;
        let pressure = 0;
        for (let j = i; j < i + 24; j++) {
            max = Math.max(max, temperatures[j]);
            pressure += pressures[j];
        }
        pressure /= 24;
        daily.pressures.push(pressure * 100);
        daily.temperatures.push(max);
    }

    return daily;
}