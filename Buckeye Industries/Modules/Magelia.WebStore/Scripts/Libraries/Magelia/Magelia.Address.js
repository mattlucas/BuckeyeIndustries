Magelia.Address = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.Address.prototype = {
    config: null,
    container: null,
    countryField: null,
    regionField: null,
    nextTrigger: null,

    initialize: function () {
        this.countryField = $(this.config.countryFieldSelector, this.container);
        this.regionField = $(this.config.regionFieldSelector, this.container);
        this.nextTrigger = $(this.config.nextTriggerSelector, this.container);
        this.setEventHandlers();
        this.refreshRegionsVisibility();
    },

    setEventHandlers: function () {
        this.countryField.change($.proxy(this.countryChanged, this));
        this.nextTrigger.click($.proxy(this.triggerNext, this));
    },

    triggerNext: function () {
        this.container.trigger('next');
    },

    countryChanged: function () {
        var countryId = this.countryField.val();
        if (countryId) {
            $.ajax(
                {
                    type: 'get',
                    data: { countryId: countryId },
                    url: this.config.getRegionsUrl,
                    success: $.proxy(
                        function (regions) {
                            this.clearRegions();
                            if ($.isArray(regions)) {
                                for (var i = 0; i < regions.length; i++) {
                                    var region = regions[i];
                                    $('<option/>').text(region.name).attr({ value: region.regionId }).appendTo(this.regionField);
                                }
                            }
                            this.refreshRegionsVisibility();
                        },
                        this
                    )
                }
            );
        }
        else {
            this.clearRegions();
            this.refreshRegionsVisibility();
        }
    },

    clearRegions: function () {
        this.regionField.children().filter(function () { return $(this).attr('value') }).remove();
        this.regionField.val('');
    },

    hasRegions: function () {
        var regions = this.regionField.children();
        return regions.length > 1 || (regions.length == 1 && regions.first().attr('value'));
    },

    refreshRegionsVisibility: function () {
        var parent = this.regionField.parent();
        if (this.hasRegions()) {
            parent.show();
        }
        else {
            parent.hide();
        }
    }
};