var app = {};

// Services

app.event = function (eventName, eventTest, eventTarget, prepareEvent) {
    /**
     * Wraps a HTML event.
     */
    if (eventTest && !eventTest()) {
        return {
            supported: false
        };
    }

    var notifier = null;

    var listen = function (e) {
        if (notifier === null) {
            return;
        }

        var arg = prepareEvent ? prepareEvent(e, notifier) : e;
        if (arg) {
            notifier.notify(arg);
        }
    }

    var start = function () {
        notifier = $.Deferred();
        eventTarget.addEventListener(eventName, listen);
    }

    var stop = function () {
        eventTarget.removeEventListener(eventName, listen);
        notifier = null;
    }

    return {
        supported: true,
        start: function () {
            if (notifier === null) {
                start();
            }

            return notifier.promise();
        },
        stop: function () {
            if (notifier !== null) {
                stop();
            }
        }
    };
};

app.event.chain = function (event, prepareEvent) {
    /**
     * Chains a new event onto a previous event.
     *
     * Because of how Deferred/Promise works, there is no 'unregister' option, so chained events don't have a 'stop' option.
     *
     * Because of how Deferred/Promise is implemented in jQuery, the chained event's promise cannot be modified, so a new
     * Notifier must be used, to allow changing the parameters.
     */

    var notifier = null;

    var start = function () {
        notifier = $.Deferred();
        event.start()
            .progress(function (e) {
                var arg = prepareEvent ? prepareEvent(e) : e;
                if (arg) {
                    notifier.notify(arg);
                }
            });
    };

    return {
        start: function () {
            if (notifier === null) {
                start();
            }
            return notifier;
        }
    };
};

app.orientation = (function () {
    var EVENT_NAME = 'deviceorientation';

    var lastOrientation = {
        alpha: null,
        beta: null,
        gamma: null
    };

    var eventTest = function () {
        return !!window.DeviceOrientationEvent;
    };

    var prepareEvent = function (e) {
        lastOrientation.alpha = e.alpha;
        lastOrientation.beta = e.beta;
        lastOrientation.gamma = e.gamma;
        return lastOrientation;
    };

    var service = app.event(EVENT_NAME, eventTest, window, prepareEvent);
    Object.defineProperty(service, 'orientation', {
        get: function () {
            return lastOrientation;
        }
    });
    return service;
}());

app.screenOrientation = (function () {
    var supported = 'orientation' in window;

    return {
        supported: supported
    };
}());

app.events = (function () {
    var account = null;
    var lastUpdate = null;
    var intervalId = null;
    var updating = false;
    
    var update = function (data) {
        var deferred = $.Deferred();

        var command =
        {
            playerid: account.group === 1 ? 'red' : 'white',
            motionvector:
            {
                x: data.beta / 30.0,
                y: data.gamma / -30.0
            }
        }

        var xhr = new XMLHttpRequest();
        xhr.open('POST', account.serviceUri + 'messages', true);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.setRequestHeader('Authorization', account.signature);
        xhr.onreadystatechange = function () {
            if (this.readyState !== 4) {
                return;
            }
            if (this.status === 201) {
                deferred.resolve();
            } else {
                deferred.reject();
            }
        }
        xhr.send(JSON.stringify(command));

        return deferred.promise();
    }

    var start = function () {
        intervalId = setInterval(function () {
            if (!lastUpdate) {
                clearInterval(intervalId);
                intervalId = null;
                return;
            }

            if (updating) {
                return;
            }

            updating = true;
            var data = lastUpdate;
            lastUpdate = null;
            update(data)
                .always(function () {
                    updating = false;
                });
        }, 100);
    }

    return {
        join: function (email, group) {
            return $.ajax({
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ Email: email }),
                url: '/api/join/'
            }).then(function (result) {
                    account = {
                        email: email,
                        group: group,
                        serviceUri: result.ServiceUri,
                        signature: result.Signature
                    };
                    return {
                        email: email,
                        group: group
                    };
                });
        },
        update: function (beta, gamma) {
            lastUpdate = {
                beta: beta,
                gamma: gamma
            };

            if (intervalId === null) {
                start();
            }
        },
        clearUpates: function () {
            if (intervalId === null) {
                return;
            }

            clearInterval(intervalId);
        }
    };
}());

app.updateHandlers = (function () {
    var onUpdate = function (x, y) {
        app.indicator.update(x, y);
        app.events.update(x, y);
    }

    var onPortraitUpdate = function (upDown, leftRight) {
        onUpdate(leftRight, upDown);
    };

    var onReversePortraitUpdate = function (upDown, leftRight) {
        onUpdate(-leftRight, -upDown);
    };

    var onLandscapeLeftUpdate = function (upDown, leftRight) {
        onUpdate(upDown, -leftRight);
    };

    var onLandscapeRightUpdate = function (upDown, leftRight) {
        onUpdate(-upDown, leftRight);
    };

    var updateHandlers = {};
    updateHandlers[0] = onPortraitUpdate;
    updateHandlers[90] = onLandscapeLeftUpdate;
    updateHandlers[-90] = onLandscapeRightUpdate;
    updateHandlers[180] = onReversePortraitUpdate;
    updateHandlers[-180] = onReversePortraitUpdate;

    var supported = 'orientation' in window;
    if (supported) {
        return {
            orientationSupported: supported,
            update: function (upDown, leftRight) {
                updateHandlers[window.orientation](upDown, leftRight);
            }
        };
    } else {
        return {
            orientationSupported: supported,
            update: function (upDown, leftRight) {
                onPortraitUpdate(upDown, leftRight);
            }
        };
    }
}());

// Directives

app.preloader = (function () {
    var element = $('#preloader');

    return {
        toggle: function () {
            element.toggleClass('visible');
        }
    };
}());

app.indicator = (function () {
    var element = $('svg #indicator')[0];

    return {
        update: function (x, y) {
            element.setAttribute('cx', x * 3);
            element.setAttribute('cy', y * 3);
        }
    };
}());

app.sleep = (function () {
    var element = document.getElementById('sleeper');
    var trigger = $('[type=submit]');

    trigger.on('click', function (e) {
        element.play();
    });
}());

// Controllers

// AppController
app.container = (function () {
    var element = $('.content');

    var _onShow;

    return {
        register: function (onShow) {
            _onShow = onShow;
        },
        showNext: function (args) {
            element.addClass('show-second');
            _onShow(args);

            // Removes animation so rotating the screen won't trigger the animation
            setTimeout(function () {
                element.removeClass('animate');
            }, 1000);
        }
    };
}());

// FirstPageController
app.join = (function () {
    var element = $('#first-page');

    if (!app.orientation.supported) {
        element.addClass('orientation-not-supported');
        return;
    }

    if (!app.screenOrientation.supported) {
        element.addClass('screen-orientation-not-supported');
    }

    element
        .find('input[name=group]')
        .prop('checked', Math.random() > 0.5);

    element
        .find('form')
        .on('submit', function (e) {
            e.preventDefault();

            app.preloader.toggle();

            var email = e.target.elements[0].value;
            var groupId = !e.target.elements[1].checked ? 1 : 2;

            app.events.join(email, groupId)
                .always(function () {
                    app.preloader.toggle();
                })
                .then(function (result) {
                    $('input:focus').blur();
                    app.container.showNext(result);
                });
        });
}());

// SecondPageController
app.experiment = (function () {
    var element = $('#second-page');

    var orientationProgress;

    var constrainRange = function (value) {
        value = value * 2;
        if (value < -30) {
            value = -30;
        } else if (value > 30) {
            value = 30;
        }
        return value;
    };

    var onOrientationChanged = function (e) {
        var upDown = constrainRange(e.beta);
        var leftRight = constrainRange(e.gamma);

        app.updateHandlers.update(upDown, leftRight);
    };

    var onShow = function (result) {
        element.addClass('team-' + result.group);

        orientationProgress = app.orientation
            .start()
            .progress(onOrientationChanged);
    };

    app.container.register(onShow);
}());
