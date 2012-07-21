Magelia.CurrencyPicker = function (container, config) {
    this.config = config;
    this.container = container;
    this.initialize();
};

Magelia.CurrencyPicker.prototype = {
    config: null,
    container: null,
    currencyField: null,

    initialize: function () {
        this.currencyField = $(this.config.currencyFieldSelector, this.container);
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        this.currencyField.change($.proxy(this.currencyChanged, this));
    },

    currencyChanged: function () {
        Magelia.Helpers.submitData(this.container, this.config.updateCurrencyUrl, true);
    }
};