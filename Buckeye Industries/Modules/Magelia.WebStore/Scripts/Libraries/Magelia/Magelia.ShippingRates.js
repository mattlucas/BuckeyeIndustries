Magelia.ShippingRates = function (container) {
    this.container = container;
    this.initialize();
};

Magelia.ShippingRates.prototype = {
    nextButton: null,
    container: null,

    initialize: function () {
        this.nextButton = $('.mag-next', this.container);
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        this.nextButton.click($.proxy(this.triggerNext, this));
    },

    triggerNext: function () {
        this.container.trigger('next');
    }
};