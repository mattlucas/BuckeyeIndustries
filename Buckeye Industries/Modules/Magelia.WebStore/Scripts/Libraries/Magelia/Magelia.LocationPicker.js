Magelia.LocationPicker = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.LocationPicker.prototype = {
    config: null,
    container: null,
    antiForgeryToken: null,
    countryField: null,
    regionField: null,

    initialize: function () {
        this.countryField = $(this.config.countryFieldSelector, this.container);
        this.regionField = $(this.config.regionFieldSelector, this.container);
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        var locationChangedProxy = $.proxy(this.locationChanged, this);
        this.countryField.change(locationChangedProxy);
        this.regionField.change(locationChangedProxy);
        $(Magelia).one('locationChanged', $.proxy(this.updateLocation, this));
    },

    updateLocation: function () {
        if (this.config.getCurrentLocationUrl) {
            $.ajax(
                {
                    type: 'get',
                    url: this.config.getCurrentLocationUrl,
                    success: $.proxy(
                        function (html) {
                            var locationPicker = $(html).insertAfter(this.container);
                            Magelia.Helpers.removeWrapper(locationPicker);
                            this.container.remove();
                        },
                        this
                    )
                }
            );
        }
    },

    locationChanged: function () {
        Magelia.Helpers.submitData(this.container, this.config.updateLocationUrl, true);
    }
};