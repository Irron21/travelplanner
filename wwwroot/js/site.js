﻿let destinationCount = 0;
function addDestination() {
    destinationCount++;
    const destinationsDiv = document.getElementById('destinations');

    let destinationHTML = `
                <div class="border p-3 mb-3 bg-white shadow-sm" id="destination_${destinationCount}">
                    <h5>Destination ${destinationCount}</h5>
                    <div class="mb-2 d-flex gap-2">        
                        <label class="form-label">City</label>
                        <input type="text" class="form-control" name="destination_city_${destinationCount}" required> 
                        <label class="form-label">State/Province</label>
                        <input type="text" class="form-control" name="destination_stateprovince_${destinationCount}" required>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Arrival Date</label>
                        <input type="date" class="form-control" id="destination_arrival_date_${destinationCount}" name="destination_arrival_date_${destinationCount}" required>
                    </div> 
                    <div class="mb-3">
                        <label class="form-label">Departure Date</label>
                        <input type="date" class="form-control" id="destination_departure_date_${destinationCount}" name="destination_departure_date_${destinationCount}" required>
                    </div>           
                    <div class="mb-3">
                    <label class="form-label">Notes</label>
                    <textarea name="Text1" class="form-control" cols="40" rows="5"></textarea>
                    </div>
                    <hr>
                    <div class="mb-3">
                        <label class="form-label">Activities</label>
                        <div id="activities_${destinationCount}">
                            <!-- Activities will be added here dynamically -->
                        </div>
                        <button type="button" class="btn btn-sm btn-secondary" onclick="addActivity(${destinationCount})">Add Activity</button>
                    </div>
                    <button type="button" class="btn btn-danger btn-sm" onclick="removeElement('destination_${destinationCount}')">Remove Destination</button>
                </div>
            `;

    destinationsDiv.insertAdjacentHTML('beforeend', destinationHTML);
}

function addActivity(destinationId) {
    let activitiesDiv = document.getElementById(`activities_${destinationId}`);
    let activityCount = document.querySelectorAll(`#activities_${destinationId} div`).length + 1;
    let activityHTML = `
        <div class="mb-4 d-flex gap-4">
            <select id="activity_${destinationId}_${activityCount}_day"></select>
            <input type="text" class="form-control" name="activity_${destinationId}[]" placeholder="Activity Name" required>
            <input type="time" class="form-control" name="activity_start_time_${destinationId}_${activityCount}" required>
            <input type="time" class="form-control" name="activity_end_time_${destinationId}_${activityCount}" required> 
            <button type="button" class="btn btn-danger btn-sm" onclick="this.parentElement.remove()">Remove</button>
        </div>
    `;

    activitiesDiv.insertAdjacentHTML('beforeend', activityHTML);

    setTimeout(() => calculateDestinationDate(destinationId, activityCount), 0);
}

function calculateDestinationDate(destinationId, activityCount) {
    let startDate = document.getElementById(`destination_arrival_date_${destinationId}`).value;
    let endDate = document.getElementById(`destination_departure_date_${destinationId}`).value;

    if (startDate && endDate) {
        let start = new Date(startDate);
        let end = new Date(endDate);
        let differenceInDays = (end - start) / (1000 * 3600 * 24);
        populateDays(differenceInDays + 1, destinationId, activityCount);
    }
}

function populateDays(totalDays, destinationId, activityCount) {
    let daySelect = document.getElementById(`activity_${destinationId}_${activityCount}_day`);
    if (!daySelect) return;

    daySelect.innerHTML = '<option value="" disabled selected>Select a day</option>';
    for (let i = 1; i <= totalDays; i++) {
        let option = document.createElement("option");
        option.value = `Day ${i}`;
        option.textContent = i;
        daySelect.appendChild(option);
    }
}


function removeElement(id) {
    document.getElementById(id).remove();
}

document.getElementById('tripForm').addEventListener('submit', function (event) {
    event.preventDefault();
    alert('Trip Saved Successfully!');
});
async function loadCountries() {
    const response = await fetch("https://restcountries.com/v3.1/all");
    const countries = await response.json();
    const countrySelect = document.getElementById("trip_country");

    countries.sort((a, b) => a.name.common.localeCompare(b.name.common));

    countries.forEach(country => {
        let option = document.createElement("option");
        option.value = country.name.common;
        option.textContent = country.name.common;
        countrySelect.appendChild(option);
    });
}

document.addEventListener("DOMContentLoaded", loadCountries);

/*function checkTripFields() {
    const tripName = document.getElementById("trip_name").value.trim();
    const tripCountry = document.getElementById("trip_country").value;
    const tripStart = document.getElementById("trip_start").value;
    const tripEnd = document.getElementById("trip_end").value;

    const isFilled = tripName !== "" && tripCountry !== "" && tripStart !== "" && tripEnd !== "";

    document.getElementById("addDestinationBtn").disabled = !isFilled;
}

document.getElementById("trip_name").addEventListener("input", checkTripFields);
document.getElementById("trip_country").addEventListener("change", checkTripFields);
document.getElementById("trip_start").addEventListener("input", checkTripFields);
document.getElementById("trip_end").addEventListener("input", checkTripFields);
*/