(function ($) {

    // jQuery.validation.unobtrusive integration with Bootstrap.  Highlights error text boxes and displays message as a tool tip
    //
    var classes = { groupIdentifier: ".form-group", error: 'has-error', success: null };//success: 'has-success' 
    function updateClasses(inputElement, toAdd, toRemove) {
        var group = inputElement.closest(classes.groupIdentifier);
        if (group.length > 0) {
            group.addClass(toAdd).removeClass(toRemove);
        }
    }
    function onError(inputElement, message) {
        updateClasses(inputElement, classes.error, classes.success);

        var options = { html: true, title: '<div class="tooltip-alert alert-danger">' + message + '</div>', placement: 'top' };
        inputElement.tooltip("destroy")
            .addClass("error")
            .tooltip(options);
    }
    function onSuccess(inputElement) {
        updateClasses(inputElement, classes.success, classes.error);
        inputElement.tooltip("destroy");
    }

    function onValidated(errorMap, errorList) {
        $.each(errorList, function () {
            onError($(this.element), this.message);
        });

        if (this.settings.success) {
            $.each(this.successList, function () {
                onSuccess($(this));
            });
        }
    }

    // document loaded
    //
    $(function () {
        // form functions.  Initialize date picker, set maxlength based on MVC StringLength attribute, and initialize jquery.validate override
        //
        $('#eventDatePicker').datetimepicker({ format: "MM/DD/YYYY", minDate: new Date(), keepInvalid: false, focusOnShow: false, useCurrent: false });
        $('input[data-val-length-max]').each(function () {
            $(this).attr('maxlength', $(this).attr('data-val-length-max'));
        });
        $('.control-label.required').each(function () {
            $(this).html($(this).html() + '<sup><span class=\'glyphicon glyphicon-asterisk required\'></span></sup>');
        });
        if (screen.width >= 768) {
            if (window != window.top) {
                $('#outFacebook').show();
            } else {
                $('#inFacebook').show();
            }
        }

        $('form').each(function () {
            var validator = $(this).data('validator');
            validator.settings.showErrors = onValidated;
        });

        // Panel collapse functions on form
        //
        $('.panel-group.form, .panel-group.event').on('show.bs.collapse', function () {
            $(this).find('.panel-heading span.glyphicon.glyphicon-menu-down').removeClass('glyphicon-menu-down').addClass('glyphicon-menu-up');
            if ($(this).find('.event-participants').length > 0) {
                var eventList = $(this).find('.event-participants');
                eventList.html('<img src=\'' + ajaxUrls.AjaxLoader + '\' />');
                $.ajax({
                    url: ajaxUrls.ParticipantsUrl,
                    type: 'POST',
                    data: { EventId: $(this).attr('data-event-id') },
                    dataType: 'json'
                }).done(function (result) {
                    eventList.html(result.Attendees);
                }).fail(function (jqXHR, textStatus) {
                    eventList.html("Unable to retrieve participant list.  Status: " + textStatus);
                });
            }
        }).on('hide.bs.collapse', function () {
            $(this).find('.panel-heading span.glyphicon.glyphicon-menu-up').removeClass('glyphicon-menu-up').addClass('glyphicon-menu-down');
        }).on('shown.bs.collapse', function () {
            $(this).find('input[type=text]').first().focus();
            var offScreen = ($(this).offset().top + $(this).height()) - ($(window).scrollTop() + $(window).innerHeight())
            if (offScreen > 0) {
                $("html, body").animate({ scrollTop: $(window).scrollTop() + offScreen + 20 }, "fast");
            }
        });

        // Attend/Not Attend status update
        //
        $('.panel-group.event').on('click', '.event-button', function () {
            var eventButton = $(this);
            var className = 'glyphicon-unchecked';
            var eventId = eventButton.attr('data-event-id');
            var status = eventButton.children('.status').html();

            if (eventButton.hasClass('event-attending')) {
                className = 'glyphicon-check';
            }

            eventButton.children('.'+className).removeClass(className).addClass('glyphicon-hourglass');
            eventButton.children('.status').html('Please wait');

            $.ajax({
                url: ajaxUrls.AttendingUrl,
                type: 'POST',
                data: { EventId: eventId, ClassName: className },
                dataType: 'json'
            }).done(function (result) {
                eventButton.parent().html(result.Button);
            }).fail(function (jqXHR, textStatus) {
                eventButton.children('.glyphicon-hourglass').removeClass('glyphicon-hourglass').addClass(className);
                if (className == 'glyphicon-unchecked') {
                    eventButton.children('.status').html(status);
                } else {
                    eventButton.children('.status').html(status);
                }
            });
        });

        // Keep the event form on the page
        //
        $(window).on('scroll', function () {
            if ($(window).width() >= 768) {
                if ($(window).scrollTop() > 80) {
                    $('#event-form').css('margin-top', ($(window).scrollTop() - 60).toString() + 'px');
                } else {
                    $('#event-form').css('margin-top', '20px');
                }
            }
        });
    });
}(jQuery));
