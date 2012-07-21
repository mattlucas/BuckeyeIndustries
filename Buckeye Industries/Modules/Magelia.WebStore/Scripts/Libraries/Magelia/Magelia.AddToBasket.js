Magelia.AddToBasket = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.AddToBasket.prototype = {
    config: null,
    container: null,
    quantityField: null,
    addToBasketTrigger: null,
    lastQuantity: null,

    initialize: function () {
        this.addToBasketTrigger = $(this.config.addToBasketTriggerSelector, this.container);
        this.quantityField = $(this.config.quantityFieldSelector, this.container);
        this.saveLastQuantity();
        this.setEventHandlers();
    },

    saveLastQuantity: function () {
        this.lastQuantity = this.quantityField.val();
    },

    setEventHandlers: function () {
        this.addToBasketTrigger.click($.proxy(this.addToBasket, this));
        this.quantityField.keyup($.proxy(this.quantityChanged, this));
    },

    quantityChanged: function () {
        var rawQuantity = this.quantityField.val();
        var quantity = parseInt(rawQuantity);
        if (isNaN(quantity) || quantity < 1) {
            quantity = this.lastQuantity;
        }
        if (rawQuantity != quantity) {
            this.quantityField.val(quantity);
        }
        this.saveLastQuantity();
    },

    parseMessage: function (message, addedQuantity, totalQuantity) {
        return (message || '').replace('{addedQuantity}', addedQuantity).replace('{totalQuantity}', totalQuantity);
    },

    showSuccess: function (addedQuantity, totalQuantity) {
        alert(this.parseMessage(this.config.successMessage, addedQuantity, totalQuantity));
    },

    showPartial: function (addedQuantity, totalQuantity) {
        alert(this.parseMessage(this.config.partialMessage, addedQuantity, totalQuantity));
    },

    showUnavailable: function () {
        alert(this.config.unavailableMessage);
    },

    showError: function () {
        alert(this.config.errorMessage);
    },

    addToBasket: function (e) {
        e.preventDefault();
        this.quantityChanged();
        var quantity = this.lastQuantity;
        if (quantity > 0 && this.config.addToBasketUrl) {
            $.ajax(
                {
                    type: 'post',
                    url: this.config.addToBasketUrl,
                    data: Magelia.Helpers.getData(this.container),
                    success: $.proxy(
                        function (result) {
                            $(Magelia).trigger('basketChanged');
                            if ($.isPlainObject(result) && result.success) {
                                if (result.addedQuantity <= 0) {
                                    this.showUnavailable();
                                }
                                else if (result.addedQuantity < quantity) {
                                    this.showPartial(result.addedQuantity, result.totalQuantity);
                                }
                                else if (result.addedQuantity == quantity) {
                                    this.showSuccess(result.addedQuantity, result.totalQuantity);
                                }
                            }
                            else {
                                this.showError();
                            }
                        },
                        this
                    ),
                    error: $.proxy(this.showError, this)
                }
            );
        }
    }
};