// initial load fct
$(document).ready(function () {

    // fct to call training in translate form via ajax (shortcut traditional form submit)
    $("#trainform").submit(function (e) {

        // add animation
        $("#SentenceIn").addClass('loading');

        //
        var $form = $(this);

        $.ajax({

            type: "POST",
            url: $form.attr("action"),
            data: $form.serialize()

        }).done(function (data) {

            // remove animation
            $("#SentenceIn").removeClass('loading');

            // report if any errors

            // format entities
            // limit to concerned text entities
            var textEntities = data.M_ARABICDARIJAENTRY_TEXTENTITYs;
            var entitiesString = "";
            for (let i = 0; i < textEntities.length; i++) {
                var textEntity = textEntities[i];
                var badgeCounter = textEntity.TextEntity.Count > 1 ? "(" + textEntity.TextEntity.Count + ")" : "";
                if (textEntities.indexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (textEntities.indexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (textEntities.indexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
            }

            // fromat entities type
            // limit to concerned text entities
            var entitiesTypesString = "";
            for (let i = 0; i < textEntities.length; i++) {
                var textEntity = textEntities[i];
                if (textEntities.indexOf(textEntity) % 4 == 0)
                    entitiesTypesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Type + "</span> ";
                else if (textEntities.indexOf(textEntity) % 4 == 1)
                    entitiesTypesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Type + "</span> ";
                else if (textEntities.indexOf(textEntity) % 4 == 2)
                    entitiesTypesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Type + "</span> ";
                else
                    entitiesTypesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Type + "</span> ";
            }

            // replace items with the returned data
            $('table.posts.table.table-striped.table-hover.table-bordered').show();
            $('table.posts.table.table-striped.table-hover.table-bordered > tbody > tr > td.arabicdarija').html(data.M_ARABICDARIJAENTRY.ArabicDarijaText);
            $('table.posts.table.table-striped.table-hover.table-bordered > tbody > tr > td.entitiestype').html(entitiesTypesString);
            $('table.posts.table.table-striped.table-hover.table-bordered > tbody > tr > td.entities').html(entitiesString);
        });
    });
});