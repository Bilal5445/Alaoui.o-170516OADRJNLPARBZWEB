$(function () {

    var setSentimentResult = function(label, score) {
        var sentimentStatus = label;
        var sentimentScore = score;

        $('#sentiment').text(sentimentStatus.toUpperCase());
        $('#sentiment-score').text(sentimentScore);
    }

    var setEntitiesResult = function(entities) {
        var entitiesHtml = '';

        entities.forEach(function (entity) {
            entitiesHtml += '<div class="entity">' + entity.Normalized + '</div>';
        });

        $('#entities').append(entitiesHtml);
    }

    var analyzeText = function() {

        var arabicText = $("#arabic-text").val();
        $('#sentiment').text('');
        $('#sentiment-score').text('');
        $('#entities').html('');

        if (arabicText) {

            $('.loader').show();

            $.ajax({
                type: "POST",
                url: "/Home/ProcessText",
                data: { text: arabicText },
                dataType: "json",
                success: function (response) {

                    $('.loader').hide();

                    var sentiment = response.Sentiment;
                    if (sentiment) {
                        setSentimentResult(sentiment.Label, sentiment.Score);
                    }

                    var entities = response.Entities;
                    if (entities) {
                        setEntitiesResult(entities);
                    }
                },
                failure: function(response) {
                    console(response);
                },
                error: function(response) {
                    console(response);
                }
            });
        }
    }

    $("#arabic-text").change(function () {
        analyzeText();
    });

    $("#analyze").click(function() {
        analyzeText();
    });
});