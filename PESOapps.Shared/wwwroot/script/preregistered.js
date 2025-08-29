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