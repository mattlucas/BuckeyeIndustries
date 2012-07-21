Magelia.Basket = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.Basket.prototype = {
    config: null,
    container: null,
    except: null,
    clearMessageTimeout: null,

    initialize: function () {
        this.except = false;
        this.setEventHandlers();
        this.getBasket();
    },

    setEventHandlers: function () {
        $(Magelia).bind('basketChanged', $.proxy(this.getBasket, this));
    },

    setBasketEventHandlers: function () {
        var self = this;
        $(this.config.quantityFieldSelector, this.container).each(
            function () {
                var input = $(this);
                input.data('initialQuantity', input.val());
            }
        ).keypress(
            function (e) {
                if (e.which == 13) {
                    e.preventDefault();
                    e.stopPropagation();
                    $(this).change();
                }
            }
        ).change(
            function (e) {
                self.quantityChanged($(this));
            }
        );
        $(this.config.removeProductTriggerSelector, this.container).click(function () { self.removeProduct($(this)); });
        $(this.config.removePromoCodeTriggerSelector, this.container).click(function () { self.removePromoCode($(this)); });
        var addPromoCodeTrigger = $(this.config.addPromoCodeTriggerSelector, this.container).click(
            function () {
                self.addPromoCode($(this));
            }
        );
        addPromoCodeTrigger.parent().find('input').keypress(
            function (e) {
                if (e.which == 13) {
                    e.preventDefault();
                    e.stopPropagation();
                    addPromoCodeTrigger.click();
                }
            }
        );
        this.clearMessageTimeout = setTimeout(function () { $(self.config.messageContainerSelector, self.container).remove(); }, 2000);
        $(this.config.updateBasketTriggerSelector, this.container).click(function () { self.updateBasket(self.config.updateBasketUrl); });
        $(this.config.clearBasketTriggerSelector, this.container).click(function () { self.updateBasket(self.config.clearBasketUrl); });
    },

    updateBasket: function (url, post, data) {
        if (url) {
            $.ajax(
                {
                    type: post ? 'post' : 'get',
                    url: url,
                    data: data,
                    success: $.proxy(
                        function (html) {
                            this.except = true;
                            $(Magelia).trigger('basketChanged');
                            this.updateView(html);
                            this.except = false;
                        },
                        this
                    ),
                    error: function () { $(Magelia).trigger('basketChanged'); }
                }
            );
        }
    },

    submitParentContainer: function (url, element) {
        this.updateBasket(url, true, Magelia.Helpers.getData(element.parent()));
    },

    addPromoCode: function (trigger) {
        this.submitParentContainer(this.config.addPromoCodeUrl, trigger);
    },

    removePromoCode: function (trigger) {
        this.submitParentContainer(this.config.removePromoCodeUrl, trigger);
    },

    removeProduct: function (trigger) {
        trigger.parents('tr').first().find(this.config.quantityFieldSelector).val(0).change();
    },

    quantityChanged: function (input) {
        var quantity = parseInt(input.val());
        if (isNaN(quantity) || quantity < 0) {
            input.val(input.data('initialQuantity'));
        }
        else {
            input.val(quantity);
            if (quantity != input.data('initialQuantity')) {
                this.submitParentContainer(this.config.updateProductQuantityUrl, input);
            }
        }
    },

    getBasket: function () {
        if (!this.except) {
            this.updateBasket(this.config.getBasketUrl);
        }
    },

    updateView: function (html) {
        clearTimeout(this.clearMessageTimeout);
        this.container.html(html);
        this.setBasketEventHandlers();
    }
};