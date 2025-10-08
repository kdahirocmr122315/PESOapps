window.initFlatpickr = function (dotnetHelper) {
    flatpickr("#fromDate", {
        dateFormat: "Y-m-d",
        onChange: function (selectedDates, dateStr) {
            dotnetHelper.invokeMethodAsync("OnFromDateSelected", dateStr);
        }
    });

    flatpickr("#toDate", {
        dateFormat: "Y-m-d",
        onChange: function (selectedDates, dateStr) {
            dotnetHelper.invokeMethodAsync("OnToDateSelected", dateStr);
        }
    });
};

window.initDateOfBirthPicker = function (dotNetHelper) {
    console.log("🟢 initDateOfBirthPicker called"); // check if Blazor invoked it

    if (!window.flatpickr) {
        console.error("❌ Flatpickr not loaded!");
        return;
    }

    const input = document.querySelector("#DateOfBirth");
    if (!input) {
        console.warn("⚠️ #DateOfBirth input not found in DOM yet!");
        return;
    }

    console.log("✅ Found #DateOfBirth input, initializing Flatpickr...");

    flatpickr(input, {
        dateFormat: "Y-m-d",
        maxDate: "today", // Prevent future dates
        onChange: function (selectedDates, dateStr) {
            console.log("📅 Date selected:", dateStr); // see what user picked
            dotNetHelper.invokeMethodAsync("UpdateDateOfBirth", dateStr);
        }
    });

    console.log("✨ Flatpickr initialization complete for #DateOfBirth");
};
