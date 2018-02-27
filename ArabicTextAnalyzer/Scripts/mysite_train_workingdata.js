// initial load fct
$(document).ready(function () {

    // bring new data into the partial view (table of posts)
    var partialViewType_workingDataOnly = 1;
    var adminModeShowAll = false;
    BringNewDataIntoPartialView(adminModeShowAll, partialViewType_workingDataOnly);
});