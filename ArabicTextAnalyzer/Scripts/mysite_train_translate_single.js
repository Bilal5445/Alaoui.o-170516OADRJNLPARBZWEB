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
            if (data.status == false) {
                // show misc area error msg
                $('#miscareaerror').css('display', 'block');
                $('#miscareaerror p').html(data.message);
                return;
            }

            // format entities
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
            $('table.posts.table.table-striped.table-hover.table-bordered > tbody > tr > td.arabicdarija div textarea').html(data.M_ARABICDARIJAENTRY.ArabicDarijaText);
            $('table.posts.table.table-striped.table-hover.table-bordered > tbody > tr > td.entitiestype').html(entitiesTypesString);
            $('table.posts.table.table-striped.table-hover.table-bordered > tbody > tr > td.entities').html(entitiesString);

            // fill all ners
            // $('#allners').html(data.M_ARABICDARIJAENTRY.ArabicDarijaText.split(/[ ,]+/).join(','));
            $.each(data.M_ARABICDARIJAENTRY.ArabicDarijaText.split(/[ ,]+/), function (index, value) {
                $('#allners').tagsinput('add', value);
                console.log(value);
            });

            // save arabic darija entry id into the button save contrib
            $('.arabicdarija div.input-group span.input-group-addon.btn').attr('data-idarabicdarijaentry', data.M_ARABICDARIJAENTRY.ID_ARABICDARIJAENTRY);
        });
    });

    // fct to keep track of changed conytrib textarea
    $("#improvetranslation").on('change keyup paste', function () {

        $(".arabicdarija > .input-group > .input-group-addon").removeClass("visibilityhidden");
    });

    // event on opening confirm save contrib : copy/show the improved contrib
    $(document).on('show.bs.modal', '#modalConfirmContrib', function () {
        console.log($("#improvetranslation").val());
        $("#modalConfirmContrib .modal-body .form-group #contrib").val($("#improvetranslation").val());
    })

    // event on select contrib textarea to allow to select NER
    /*$("#improvetranslation").on('contextmenu', function (e) {
        alert("right-click!");
        window.event.returnValue = false;
    });*/
});

// fct to save user contrb to better suggested translation
function saveTranslationContribution() {

    //
    $.ajax({
        "dataType": 'json',
        "type": "POST",
        "url": "/Train/SaveTranslationContributionAjax",
        "data": {
            "idarabicdarijaentry" : $('.arabicdarija div.input-group span.input-group-addon.btn').attr('data-idarabicdarijaentry'),
            "contrib": $('input#contrib').val(),
        },
        "success": function (msg) {

            // hide modal 
            $('#modalConfirmContrib').modal('toggle');

            if (msg.status) {

                // show misc area success msg
                $('#miscareasuccess').css('display', 'block');
                $('#miscareasuccess p').html(msg.message);

            } else {

                // show misc area error msg
                $('#miscareaerror').css('display', 'block');
                $('#miscareaerror p').html(msg.message);
            }
        },
        "error": function () {

            // hide modal 
            $('#modalConfirmContrib').modal('toggle');

            // show misc area error msg
            $('#miscareaerror').css('display', 'block');
            $('#miscareaerror p').html(msg.message);
        }
    });
}

