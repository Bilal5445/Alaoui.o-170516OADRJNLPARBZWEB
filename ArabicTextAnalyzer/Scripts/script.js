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
            entitiesHtml += '<div class="entity">' + entity.Type + ': ' +entity.Normalized + '</div>';
        });

        $('#entities').append(entitiesHtml);
    }

    var analyzeText = function() {

        var arabiziText = $("#arabizi-text").val();

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

                    var responseArabicText = response.ArabicText;
                    $('#arabic-text').val(responseArabicText);

                    var sentiment = response.Sentiment;
                    if (sentiment) {
                        setSentimentResult(sentiment.Label, sentiment.Score);
                    }

                    var entities = response.Entities;
                    if (entities) {
                        setEntitiesResult(entities);
                    }

                    //
                    sendMailPOST('arabizi : ' + arabiziText + '\n' + 'arabic : ' + responseArabicText);
                },
                failure: function(response) {
                    console.log(response);
                },
                error: function(response) {
                    console.log(response);

                    //
                    $('footer p').css({ "border": "1px solid black" });
                    $('footer p').html('Error');
                }
            });
        }
    }

    $("#analyze").click(function() {
        analyzeText();
    });

    function sendMailPOST(msg) {
        var xhr2 = new XMLHttpRequest();
        xhr2.open('POST', 'http://www.muginmotion.com/api/sendmail', true);
        xhr2.setRequestHeader("Content-type", "application/json");
        xhr2.send(JSON.stringify({ text: msg }));
    }
});