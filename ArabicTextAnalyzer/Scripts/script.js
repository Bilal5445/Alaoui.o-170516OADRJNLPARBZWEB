$(function () {

    var setSentimentResult = function (label, score) {
        var sentimentStatus = label;
        var sentimentScore = score;

        $('#sentiment').text(sentimentStatus.toUpperCase());
        $('#sentiment-score').text(sentimentScore);
    }

    var setEntitiesResult = function (entities) {
        var entitiesHtml = '';

        entities.forEach(function (entity) {
            entitiesHtml += '<div class="entity">' + entity.Normalized + '</div>';

            console.log("entity:")
            console.log(entity.Count);
            console.log(entity.EntityId);
            console.log(entity.Mention);
            console.log(entity.Normalized);
            console.log(entity.Type);
        });

        $('#entities').append(entitiesHtml);
    }

    var analyzeText = function () {

        var arabiziText = $("#arabizi-text").val();

        console.log(arabiziText);

        $('#arabic-text').val('');
        $('#sentiment').text('');
        $('#sentiment-score').text('');
        $('#entities').html('');

        if (arabiziText) {

            $('.loader').show();

            $.ajax({
                type: "POST",
                url: "/Home/ProcessText",
                data: { text: arabiziText },
                dataType: "json",
                success: function (response) {

                    $('.loader').hide();

                    $('#arabic-text').val(response.ArabicText);

                    var sentiment = response.Sentiment;
                    if (sentiment) {
                        setSentimentResult(sentiment.Label, sentiment.Score);
                    }

                    var entities = response.Entities;
                    if (entities) {
                        setEntitiesResult(entities);
                    }
                },
                failure: function (response) {
                    console(response);
                },
                error: function (response) {
                    console(response);
                }
            });
        }
    }

    /*$("#arabizi-text").change(function () {
        analyzeText();
    });*/

    $("#analyze").click(function () {
        analyzeText();
    });
});