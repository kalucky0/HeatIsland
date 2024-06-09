function calculateEnergyCost(energy, costPerKWh) {
    return energy / 3600000 * costPerKWh;
}

function calculateEnergy(airVolume, pressure, temperature, temperatureDelta) {
    var density = calculateAirDensity(temperature, pressure);
    var mass = density * airVolume;
    return mass * 1006 * temperatureDelta;
}

function calculateAirDensity(temperature, pressure) {
    return pressure / (287.05 * (temperature + 273.15));
}